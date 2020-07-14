using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class NavigationManager : MonoBehaviour
{
    ARTrackedImageManager arTrackedImageManager;

    [SerializeField]
    GameObject normalPrefab;

    [SerializeField]
    GameObject krogerPrefab;

    [SerializeField]
    GameObject waypoint;

    [SerializeField]
    GameObject groundMarker;

    private GameObject marker;
    private GameObject container;

    private GameObject aisleMarker;
    private GameObject aisleContainer;



    public Vector3 currentImageLocation;
    public Vector3 recordedImageLocation;
    public Vector3 markerPlacedLocation;
    public Vector3 containerLocation;
    public string childName;
    public Quaternion rot;

    public Vector3 aisleImageLocation;
    public Quaternion aisleImageRotation;

    public int count;

    private bool hasNavigationStarted;
    private bool inAisle;

    void Awake()
    {
        arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        hasNavigationStarted = false;
        inAisle = false;
    }

    private void OnEnable()
    {
        arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        UpdateTrackedImages(eventArgs.added);
        UpdateTrackedImages(eventArgs.updated);
    }

    private void UpdateTrackedImages(IEnumerable<ARTrackedImage> trackedImages)
    {
        foreach(var trackedImage in trackedImages)
        {
            if (trackedImage.trackingState != TrackingState.Limited)
            {

                trackedImage.gameObject.SetActive(true);
                if (trackedImage.referenceImage.name.Equals("kroger_logo"))
                {
                    StartMarkerFound(trackedImage);
;               }
                else if (trackedImage.referenceImage.name.Equals("chips"))
                {
                    AisleFound(trackedImage);
                }

            }
            else
            {
                trackedImage.gameObject.SetActive(false);
            }
        }
    }

    private void AisleFound(ARTrackedImage trackedImage)
    {

        String aisleName = trackedImage.referenceImage.name;
        aisleImageLocation = trackedImage.transform.position;
        aisleImageRotation = trackedImage.transform.rotation;
        switch (aisleName)
        {
            case "chips":
                if (!inAisle)
                {
                    aisleMarker = Instantiate(groundMarker, aisleImageLocation, aisleImageRotation);
                    aisleContainer = aisleMarker.transform.GetChild(0).gameObject;
                    aisleContainer.transform.localPosition = new Vector3(6f, 0f, -1.5f);
                    aisleContainer.transform.Rotate(new Vector3(90,0,0));
                    childName = aisleContainer.name;
                    markerPlacedLocation = aisleMarker.transform.position;
                    containerLocation = aisleContainer.transform.position;
                    inAisle = true;
                    marker.SetActive(false);
                }
                else
                {
                    aisleMarker.transform.position = trackedImage.transform.position;
                    aisleMarker.transform.rotation = trackedImage.transform.rotation;
                    markerPlacedLocation = aisleMarker.transform.position;
                    containerLocation = aisleContainer.transform.position;
                }
                break;
            default:
                break;
        }
    }

    private void StartMarkerFound(ARTrackedImage trackedImage)
    {

        if (!hasNavigationStarted)
        {
            recordedImageLocation = trackedImage.transform.position;
            rot = trackedImage.transform.rotation;
            marker = Instantiate(waypoint, recordedImageLocation, rot);
            container = marker.transform.GetChild(0).gameObject;
            container.transform.localPosition = new Vector3(0f, -8.9f, -.2f);
            container.transform.Rotate(new Vector3(90, 0, 0));
            hasNavigationStarted = true;
            markerPlacedLocation = marker.transform.position;

        }
        else
        {
            marker.SetActive(true);
            count++;
            marker.transform.position = trackedImage.transform.position;
            marker.transform.rotation = trackedImage.transform.rotation;
            markerPlacedLocation = marker.transform.position;
            containerLocation = container.transform.position;
        }
        currentImageLocation = trackedImage.transform.position;
    }

    private void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
