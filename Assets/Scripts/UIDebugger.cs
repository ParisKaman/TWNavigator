using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class UIDebugger : MonoBehaviour
{
    public Text text;
    NavigationManager navigationManager;
    ARTrackedImageManager arTrackedImageManager;
    ARAnchorManager aRAnchorManager;
    ARTapToPlace aRTapToPlace;

    // Start is called before the first frame update
    void Start()
    {
        navigationManager = GetComponent<NavigationManager>();
        arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        aRAnchorManager = GetComponent<ARAnchorManager>();
        aRTapToPlace = GetComponent<ARTapToPlace>();
    }

    // Update is called once per frame
    void Update()
    {

        text.text = "Anchor Count: " + aRAnchorManager.trackables.count;
        text.text += "\nList Count: " + aRTapToPlace.aRAnchors.Count;
        text.text += "\nTouch Count: " + Input.touchCount;
        if(aRTapToPlace.cloudAnchorManager.currentCloudAnchor != null)
        {
            text.text += "\nCurrentAnchor: " + aRTapToPlace.cloudAnchorManager.currentCloudAnchor.Identifier;
        }
        if (aRTapToPlace.cloudAnchorManager.previousCloudAnchor != null)
        {
            text.text += "\nPreviousAnchor: " + aRTapToPlace.cloudAnchorManager.previousCloudAnchor.Identifier;
        }
    }
}
