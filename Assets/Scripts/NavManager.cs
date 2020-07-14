using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.SpatialAnchors;
using UnityEngine;
using TMPro;

public class NavManager : MonoBehaviour
{
    CloudAnchorManager cloudAnchorManager;

    DropdownHandler dropdownHandler;

    UIManager uiManager;

    public TMP_Text topText;

    [SerializeField]
    GameObject waypointPointer;

    public CloudSpatialAnchor destinationAnchor = null;
    public Vector3 destinationLocation = new Vector3(0,0,0);
    private bool wayfinding;
    private float distanceToDestination;

    private GameObject arrow;

    // Start is called before the first frame update
    void Start()
    {
        dropdownHandler = FindObjectOfType<DropdownHandler>();
        uiManager = FindObjectOfType<UIManager>();
        cloudAnchorManager = GetComponent<CloudAnchorManager>();
        wayfinding = false;
        distanceToDestination = 0f;
        arrow = waypointPointer.transform.GetChild(0).GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (wayfinding)
        {
            distanceToDestination = Vector3.Distance(destinationLocation, Camera.main.transform.position);
            arrow.transform.LookAt(2* arrow.transform.position - destinationLocation);

            if(distanceToDestination < 1)
            {
                destinationReached();
            }
        }
        else
        {
            waypointPointer.gameObject.SetActive(false);
        }
    }


    public void BeginWayfind(string name)
    {
        destinationAnchor = cloudAnchorManager.getAnchor(name);
        GameObject destinationObject = cloudAnchorManager.getAnchorGameObject(destinationAnchor);
        destinationLocation = destinationObject.transform.position;
        waypointPointer.gameObject.SetActive(true);
        wayfinding = true;

        topText.text = "Navigating to: " + name;

        //this flips the lookat around 180 degrees so the point is facing the right direction
        arrow.transform.LookAt(2 * arrow.transform.position - destinationLocation);
        _ = cloudAnchorManager.selectAnchor(destinationObject);
    }

    public void PauseWayfinding()
    {
        wayfinding = false;
    }

    public void destinationReached()
    {
        wayfinding = false;
        waypointPointer.gameObject.SetActive(false);

        uiManager.destinationReachedUI();
    }

    public string getNextStop()
    {
        int anchorIndex = dropdownHandler.dropdownList.IndexOf(dropdownHandler.currentDropdown);
        if (anchorIndex == dropdownHandler.dropdownList.Count - 1)
        {
          topText.text = "Navigation completed!";
            //at end of dropdown list -- no more anchors left
            return "Shopping complete, have a good day!";
        }
        else
        {
            return "Next Stop: " + dropdownHandler.dropdownList[anchorIndex + 1].options[dropdownHandler.dropdownList[anchorIndex + 1].value].text;
        }
    }

    public void confirmDestination()
    {
        uiManager.activateDefaultView();
        int anchorIndex = dropdownHandler.dropdownList.IndexOf(dropdownHandler.currentDropdown);
        if(anchorIndex == dropdownHandler.dropdownList.Count - 1)
        {
            //at end of dropdown list -- no more anchors left

        }
        else
        {
            dropdownHandler.currentDropdown = dropdownHandler.dropdownList[anchorIndex + 1];
            BeginWayfind(dropdownHandler.currentDropdown.options[dropdownHandler.currentDropdown.value].text);
        }
    }
}
