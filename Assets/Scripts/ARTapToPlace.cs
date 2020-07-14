using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ntw.CurvedTextMeshPro;
using TMPro;

public class ARTapToPlace : MonoBehaviour
{
    private ARRaycastManager aRRaycastManager;
    private ARAnchorManager aRAnchorManager;
    public List<ARAnchor> aRAnchors;
    public Pose placementPose;
    volatile public bool isPlacementValid;
    public GameObject placementIndicator;
    public GameObject objectToPlace;
    public List<ARRaycastHit> hits = new List<ARRaycastHit>();
    public GameObject indicator;
    public Pose lastPosePlaced;
    public GameObject spawnedObject;
    public bool inCreateAnchor;


    public CloudAnchorManager cloudAnchorManager;
    private CloudSpatialAnchor currentCloudSpatialAnchor;

    volatile public bool tryingToUpdate;
    public bool inUpdateMethod;
    public bool validAtIndicatorStart;
    public Text topText;

    // Start is called before the first frame update
    void Start()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRAnchorManager = GetComponent<ARAnchorManager>();
        indicator = Instantiate(placementIndicator);
        currentCloudSpatialAnchor = null;
        aRAnchors = new List<ARAnchor>();
        isPlacementValid = false;
        tryingToUpdate = false;
        inCreateAnchor = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        inUpdateMethod = false;
        UpdatePlacementIndicator();

        EventSystem eventSystem = FindObjectOfType<EventSystem>();

        if (Input.touchCount > 0)
        {
            if (eventSystem == null || !eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if (isPlacementValid && Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    PlaceObject();
                }
                if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    attemptAnchorSelect();
                }
            }
        }

    }

    private void attemptAnchorSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        RaycastHit raycastHit;

        //make a hitMask so that the raycast only hits objects in the layer anchorContainers
        int hitMask = 1 << 9;
        if(Physics.Raycast(ray, out raycastHit, Mathf.Infinity, hitMask))
        {
            GameObject objectHit = raycastHit.transform.gameObject;
            topText.text = objectHit.name;
            GameObject anchorContainer = objectHit.transform.parent.gameObject;
            if (anchorContainer.gameObject.tag.Equals("AnchorContainer"))
            {
                _ = cloudAnchorManager.selectAnchor(anchorContainer);
            }
        }
    }

    public async void PlaceAnchor(string anchorLabel)
    {
      //For some reason, we need to refresh TextProOnACircle component so that it
      //doesn't unravel and just display the text in a straight line.
      //note: this is a total band-aid fix that only kind of works anyway..
      //TODO: find out why this is happening and fix it at the source so we don't
      //      have to destroy and create stuff
      ARTapToPlace arTapManager = FindObjectOfType<ARTapToPlace>();
      TextProOnACircle tpoac = arTapManager.spawnedObject.transform.Find("Anchor Text").GetComponent<TextProOnACircle>();
      TMP_Text targetText = arTapManager.spawnedObject.transform.Find("Anchor Text").GetComponent<TMP_Text>();
      targetText.text = "";
      Destroy(tpoac);
      arTapManager.spawnedObject.transform.Find("Anchor Text").gameObject.AddComponent<TextProOnACircle>();
      int anchorLabelLength = 0;
      while(anchorLabelLength <= 26)
      {
          targetText.text += anchorLabel + "    ";
          anchorLabelLength += anchorLabel.Length + 4;
      }

      //End of weird circle text refresh


        //check for the object first
        if(spawnedObject == null)
        {
            //haven't placed
            Debug.Log("Trying to make cloud anchor without local");
            return;
        }

        //gets the CloudNativeAnchor component
        CloudNativeAnchor cna = spawnedObject.GetComponent<CloudNativeAnchor>();

        //if the spawned object doesn't have a cloud anchor associated with it,
        //NativeToCloud will make one with the CloudNativeAnchor component we just got
        if(cna.CloudAnchor == null)
        {
            cna.NativeToCloud();
        }

        //Begin to convert ARFoundation anchor to a CloudSpatialAnchor
        CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;
        //set app property for label
        cloudAnchor.AppProperties["label"] = anchorLabel;
        await cloudAnchorManager.CreateCloudAnchor(cloudAnchor);

        //now the anchor has been created, need to refresh session so it shows up maybe?


        //clean up the local instance of the cube and anchor we just placed
        //so we can reset and render the version from the cloud
        CleaupSpawnedObject();

        await cloudAnchorManager.ResetSession();
        cloudAnchorManager.findAnchorsByLocation();
    }

    private void CleaupSpawnedObject()
    {
        if(spawnedObject != null)
        {
            Destroy(spawnedObject);
        }
    }

    public void EnableIndicator(bool enabled)
    {
      inCreateAnchor = enabled;
    }

    public async void DeleteAnchor()
    {
        Debug.Log("ARTTP DeleteAnchor() w/ " + currentCloudSpatialAnchor.Identifier);
        if(currentCloudSpatialAnchor != null)
        {
            await cloudAnchorManager.deleteCloudAnchor(currentCloudSpatialAnchor);

            //reset to see the changes
            await cloudAnchorManager.ResetSession();
        }

    }

    private void PlaceObject()
    {
        //instantiate objectToPlace at the placement location + a little bit so it doesn't clip ground plane too badly
        spawnedObject = Instantiate(objectToPlace, placementPose.position + new Vector3(0, 0.05f, 0), placementPose.rotation);

        //Add a cloudNativeAnchor component to the gameobject so that the manager can track it
        spawnedObject.AddComponent<CloudNativeAnchor>();
        //if(currentCloudSpatialAnchor == null)
        //{
        //    CloudNativeAnchor cloudNativeAnchor = spawnedObject.GetComponent<CloudNativeAnchor>();
        //    cloudNativeAnchor.CloudToNative(currentCloudSpatialAnchor);
        //}
        lastPosePlaced = placementPose;
    }

    private void UpdatePlacementIndicator()
    {
        inUpdateMethod = true;

        if (isPlacementValid && inCreateAnchor)
        {
            tryingToUpdate = true;
            indicator.SetActive(true);
            indicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            tryingToUpdate = false;
            indicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        isPlacementValid = hits.Count > 0;

        if (isPlacementValid)
        {

            placementPose = hits[0].pose;

            var cameraForward = Camera.main.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);

        }

    }
}
