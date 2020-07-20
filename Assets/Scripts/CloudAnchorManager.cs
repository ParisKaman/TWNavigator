using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors.Unity.Android;
using System;
using System.Linq;
using TMPro;
using ntw.CurvedTextMeshPro;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class CloudAnchorManager : MonoBehaviour
{
    List<string> cloudIDs = new List<string>();

    private CloudSpatialAnchorSession cloudSession = null;

    private SpatialAnchorManager spatialAnchorManager = null;

    private ARAnchorManager aRAnchorManager;

    [SerializeField]
    private ARSession aRSession;

    [SerializeField]
    private ARCameraManager aRCameraManager;

    [SerializeField]
    private Material highlightMaterial;
    [SerializeField]
    private Material baseMaterial;
    [SerializeField]
    private Material deleteMaterial;

    public TMP_Text topText;
    public Text debugText;

    public CloudSpatialAnchor currentCloudAnchor = null;
    public CloudSpatialAnchor previousCloudAnchor = null;

    public List<CloudSpatialAnchor> foundAnchors = new List<CloudSpatialAnchor>();


    const string androidWifiAccessPermission = "android.permission.ACCESS_WIFI_STATE";
    const string androidWifiChangePermission = "android.permission.CHANGE_WIFI_STATE";


    // Start is called before the first frame update
    async void Start()
    {
        //have to Task Delay at the start in order to give ARSession time to set up properly
        await Task.Delay(3000);
        spatialAnchorManager = GetComponent<SpatialAnchorManager>();
        aRAnchorManager = FindObjectOfType<ARAnchorManager>();

        //if the session doesn't exist, create session
        if (spatialAnchorManager.Session == null)
        {
            await spatialAnchorManager.CreateSessionAsync();
        }
        //add AnchorLocated event so when the session detects an anchor, it does what we want it to
        spatialAnchorManager.AnchorLocated += CloudAnchorManager_AnchorLocated;
        debugText.text = "\nSession Created";

        //begin the session
        await spatialAnchorManager.StartSessionAsync();

        //request the sesnsor permissions for wifi and geolocation -- then configure
        requestSensorPermissions();
        configureSensors();
        debugText.text += "\nSession Started";
        debugText.text += "\nID: " + spatialAnchorManager.SpatialAnchorsAccountId;
        debugText.text += "\nKey: " + spatialAnchorManager.SpatialAnchorsAccountKey;
    }

    private void configureSensors()
    {
        PlatformLocationProvider sensorProvider = new PlatformLocationProvider();
        spatialAnchorManager.Session.LocationProvider = sensorProvider;
        //TODO: switch these values to equal the granted perissions instead of assuming true
        sensorProvider.Sensors.GeoLocationEnabled = true;
        sensorProvider.Sensors.WifiEnabled = false;
        sensorProvider.Start();
    }

    private void CloudAnchorManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        if(args.Status == LocateAnchorStatus.Located)
        {
            Debug.Log("Found anchor: " + args.Identifier);
            if(args.Identifier != null)
            {
                foundAnchors.Add(args.Anchor);
            }
            debugText.text = "Anchor Located: " + args.Identifier;
            //shift current to previous and get new current
            if(currentCloudAnchor == null || args.Anchor.Identifier != currentCloudAnchor.Identifier)
            {
                previousCloudAnchor = currentCloudAnchor;
                currentCloudAnchor = args.Anchor;
            }


            //get a list of all gameobjects with tag AnchorContainer.  This will get a list of all anchor prefabs
            var anchorContainers = GameObject.FindGameObjectsWithTag("AnchorContainer");
            debugText.text += "\nNum Anchors: " + anchorContainers.Length;
            GameObject anchorContainer = null;


            for (int i = 0; i < anchorContainers.Length; i++)
            {
                //if anchorcontainer is at location of the current anchor, this is the one we just found
                if (anchorContainers[i].gameObject.transform.position == currentCloudAnchor.GetPose().position)
                {
                    anchorContainer = anchorContainers[i];
                    anchorContainer.gameObject.GetComponent<AnchorContainer>().anchorID = args.Anchor.Identifier;
                    debugText.text += "\nAnchor Added: " + anchorContainer.gameObject.GetComponent<AnchorContainer>().anchorID;
                }

                //if we found the previous container, we need to reset this one so it isn't highlighted
                if(previousCloudAnchor != null)
                {
                    if (anchorContainers[i].gameObject.transform.position == previousCloudAnchor.GetPose().position)
                    {
                        changeAnchorMaterials(anchorContainers[i], baseMaterial);
                    }
                }
            }

            if (anchorContainer != null)
            {
                //make all the children in the anchorcontainer visible and highlight the cyclinder
                for (int i = 0; i < anchorContainer.transform.childCount; i++)
                {
                    GameObject child = anchorContainer.transform.GetChild(i).gameObject;
                    if (!child.name.Equals("Cylinder"))
                    {
                        child.SetActive(true);
                    }
                    //if (anchorContainer.transform.GetChild(i).gameObject.name.Equals("Cylinder"))
                    //{
                    //    anchorContainer.transform.GetChild(i).gameObject.GetComponent<Renderer>().material = highlightMaterial;
                    //}

                }

                changeAnchorMaterials(anchorContainer, highlightMaterial);

                //set the curved text to the identifier
                TMP_Text containerText = anchorContainer.GetComponentInChildren<TMP_Text>();
                containerText.text = "" + currentCloudAnchor.Identifier;

                //set the curved text to the label if it exists
                //TODO: change this so that it algorithmically determines the amount of
                //      repeats and the amount of spacing so it looks good with
                //      any word
                if (currentCloudAnchor.AppProperties.ContainsKey("label") && currentCloudAnchor.AppProperties["label"] != null)
                {
                    string label = currentCloudAnchor.AppProperties["label"];
                    int anchorLabelLength = 0;
                    containerText.text = "";
                    while(anchorLabelLength <= 26)
                    {
                        containerText.text += label + "    ";
                        anchorLabelLength += label.Length + 4;
                    }
                }
                else
                {
                    Debug.Log("Anchor found with ID: " + currentCloudAnchor.Identifier);
                }
                topText.text = "Anchors located!  Localization complete!";
            }
            else
            {

                debugText.text += "No Anchor Detected";
            }
        }
    }

    public async void findAnchors()
    {
        MenuHandler menuHandler = FindObjectOfType<MenuHandler>();
        if(menuHandler != null)
        {
            menuHandler.HideMenuPanel();
        }

        Debug.Log("Watchers :" + spatialAnchorManager.Session.GetActiveWatchers().Count);
        if(spatialAnchorManager.Session.GetActiveWatchers().Count > 0)
        {
            Debug.Log("Already have watchers");
            await ResetSession();
            findAnchorsByLocation();
        }
        else
        {
            topText.text = "Attempting to localize device.  Continue to scan the area";
            findAnchorsByLocation();
        }
    }

    public CloudSpatialAnchor getAnchor(string name)
    {
        Debug.Log("CloudAnchorManager getAnchor() called w/ " + name);
        foreach(CloudSpatialAnchor csa in foundAnchors)
        {
            if (csa.AppProperties.ContainsKey("label") && csa.AppProperties["label"] != null)
            {
                if (csa.AppProperties["label"].Equals(name))
                {
                    return csa;
                }
            }
            else
            {
                if (csa.Identifier.Equals(name))
                {
                    return csa;
                }
            }
        }
        return null;
    }

    //method to find anchors based on geolocation + wifi
    public void findAnchorsByLocation()
    {
        //set a neardevicecriteria to look for anchors within 5 meters
        //can return max of 25 anchors to be searching for at once time here
        NearDeviceCriteria nearDeviceCriteria = new NearDeviceCriteria();
        nearDeviceCriteria.DistanceInMeters = 5;
        nearDeviceCriteria.MaxResultCount = 25;
        AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
        anchorLocateCriteria.NearDevice = nearDeviceCriteria;
        debugText.text = "Trying to find anchors by location";
        spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);
    }


    //specific lookup for known anchors
    public void FindCloudAnchors()
    {
        //set AnchorLocateCriteria to look for the IDs we have stored in cloudIDs list
        AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
        //anchorLocateCriteria.Identifiers = cloudIDs.ToArray();
        anchorLocateCriteria.Identifiers = new string[]{
            "7259b41a-7860-49c2-acbb-1dbeba9801c1" };

        debugText.text = "Trying to locate : " + anchorLocateCriteria.Identifiers.GetValue(0);

        //create a watcher with the criteria containing list of IDs
        //when a watcher fires, AnchorLocated event will fire
        spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);

    }

    public async Task updateCloudSpatialAnchorByID(string ID)
    {
        //loop through our list of CloudSpatialAnchors and look for the one with
        //the ID we are looking for.
        //If it isn't the same anchor, shift anchors down the line
        foreach(CloudSpatialAnchor csa in foundAnchors)
        {
            if(csa.Identifier == ID)
            {
                if (currentCloudAnchor == null || csa.Identifier != currentCloudAnchor.Identifier)
                {
                    previousCloudAnchor = currentCloudAnchor;
                    currentCloudAnchor = csa;
                }
            }
        }
    }

    async public void deleteAllCloudAnchors()
    {

    }

    async public void deleteCurrentCloudAnchor()
    {
        debugText.text = "Trying to delete anchor: " + currentCloudAnchor.Identifier;
        await spatialAnchorManager.Session.DeleteAnchorAsync(currentCloudAnchor);
        await ResetSession();
        findAnchorsByLocation();
        topText.text = "Anchor deleted. Confirming remaining anchors, scan the area";
    }

    async public Task deleteCloudAnchor(CloudSpatialAnchor cloudSpatialAnchor)
    {
        debugText.text = "Trying to delete anchor: " + cloudSpatialAnchor.Identifier;
        await spatialAnchorManager.Session.DeleteAnchorAsync(cloudSpatialAnchor);
    }

    async public Task ResetSession()
    {
        foreach (ARAnchor anchor in aRAnchorManager.trackables)
        {
            aRAnchorManager.RemoveAnchor(anchor);
            Debug.Log("trying to remove anchor: " + anchor.trackableId);
        }
        currentCloudAnchor = null;
        previousCloudAnchor = null;
        foundAnchors.Clear();
        spatialAnchorManager.StopSession();
        await spatialAnchorManager.ResetSessionAsync();
        await spatialAnchorManager.StartSessionAsync();
    }

    public async Task CreateCloudAnchor(CloudSpatialAnchor cloudSpatialAnchor)
    {
        ARTapToPlace arTapManager = FindObjectOfType<ARTapToPlace>();
        TextProOnACircle tpoac = arTapManager.spawnedObject.transform.Find("Anchor Text").GetComponent<TextProOnACircle>();


        debugText.text = "Trying to create Cloud Anchor " + cloudSpatialAnchor.Identifier;
        if(cloudSpatialAnchor is null)
        {
            debugText.text += "Anchor is null, not creating";
            return;
        }

        //while the manager doesn't have enough data to create an Azure anchor, wait for more info
        //TODO: add a better indicator for the user to gather more environmental data
        while (!spatialAnchorManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            float createProgress = spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
            topText.text = $"Move your device to capture more environment data: {createProgress:0%}";
        }
        debugText.text = "\nMade it out";

        //creates the anchor in the manager
        try
        {
            debugText.text += "\nTrying to call CreateAnchorAsync() with " + cloudSpatialAnchor.Identifier;
            await spatialAnchorManager.Session.CreateAnchorAsync(cloudSpatialAnchor);
            debugText.text += "\nAnchor created with ID: " + cloudSpatialAnchor.Identifier;
        }
        catch(Exception ex)
        {
            debugText.text = "Exception thrown: " + ex.Message;
            debugText.text += "\nWifi Enabled: " + spatialAnchorManager.Session.LocationProvider.Sensors.WifiEnabled;
            debugText.text += "\nGeo Enabled: " + spatialAnchorManager.Session.LocationProvider.Sensors.GeoLocationEnabled;
            debugText.text += "\nWifi Status: " + spatialAnchorManager.Session.LocationProvider.WifiStatus;
            debugText.text += "\nGeo Status: " + spatialAnchorManager.Session.LocationProvider.GeoLocationStatus;
        }
    }

    public void requestSensorPermissions()
    {
        #if UNITY_ANDROID
        RequestPermissionIfNotGiven(Permission.FineLocation);
        RequestPermissionIfNotGiven(Permission.CoarseLocation);

        RequestPermissionIfNotGiven(androidWifiAccessPermission);
        RequestPermissionIfNotGiven(androidWifiChangePermission);
        #endif

    }

    private static void RequestPermissionIfNotGiven(string permission)
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Debug.Log("Requesting access to " + permission);
            Permission.RequestUserPermission(permission);
        }
        else
        {
            Debug.Log("Already had access to " + permission);
        }
#endif
    }


    //we've hit a game object and we want the CloudSpatialAnchor associated with
    //it to go into currentCloudAnchor
    //then we switch the highlights to reflect that
    public async Task selectAnchor(GameObject objectHit)
    {
        var hitPosition = objectHit.transform.position;
        var anchorContainers = GameObject.FindGameObjectsWithTag("AnchorContainer");
        debugText.text = "AnchorList Length: " + anchorContainers.Length;
        foreach(GameObject anchor in anchorContainers)
        {
            if(anchor.gameObject.transform.position == objectHit.transform.position)
            {
                AnchorContainer ac = anchor.gameObject.GetComponent<AnchorContainer>();
                if(ac != null)
                {
                    Debug.Log("Selected Anchor: " + ac.anchorID);
                    debugText.text = "Selected Anchor: " + ac.anchorID;
                    //shift anchors down
                    await updateCloudSpatialAnchorByID(ac.anchorID);
                }
            }
        }

        var previousPosition = previousCloudAnchor.GetPose().position;
        for (int i = 0; i < anchorContainers.Length; i++)
        {
            //if anchorcontainer is at location of the current anchor, this is the one we just found
            if (anchorContainers[i].gameObject.transform.position == hitPosition)
            {
                changeAnchorMaterials(anchorContainers[i], highlightMaterial);
            }

            //if we found the previous container, we need to reset this one so it isn't highlighted
            if (anchorContainers[i].gameObject.transform.position == previousPosition)
            {
                changeAnchorMaterials(anchorContainers[i], baseMaterial);
                //anchorContainers[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = baseMaterial;
            }
        }
    }

    public void changeAnchorMaterials(GameObject anchorContainer, Material material)
    {
        var renderers = anchorContainer.gameObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            if(!renderer.gameObject.name.Equals("Anchor Text"))
            {
                renderer.material = material;
            }
        }
        var texts = anchorContainer.gameObject.GetComponentsInChildren<TMP_Text>();
        foreach(TMP_Text text in texts)
        {
            text.color = material.color;
        }
    }

    //Get the Unity GameObject associated with the currentCloudAnchor
    public GameObject getCurrentAnchorGameObject()
    {
        GameObject currentAnchor = null;
        var anchorContainers = GameObject.FindGameObjectsWithTag("AnchorContainer");
        foreach(GameObject anchor in anchorContainers)
        {
            AnchorContainer anchorContainer = anchor.GetComponent<AnchorContainer>();
            if(anchorContainer.anchorID == currentCloudAnchor.Identifier)
            {
                currentAnchor = anchor;
                break;
            }
        }
        return currentAnchor;
    }

    public GameObject getAnchorGameObject(CloudSpatialAnchor csa)
    {
        GameObject csaGameObject = null;
        var anchorContainers = GameObject.FindGameObjectsWithTag("AnchorContainer");
        foreach(GameObject anchor in anchorContainers)
        {
            AnchorContainer anchorContainer = anchor.GetComponent<AnchorContainer>();
            if(anchorContainer.anchorID == csa.Identifier)
            {
                csaGameObject = anchor;
                break;
            }
        }
        return csaGameObject;
    }

    public void hoverDelete()
    {
        GameObject currentAnchorContainer = getCurrentAnchorGameObject();
        changeAnchorMaterials(currentAnchorContainer, deleteMaterial);
    }

    public void cancelDelete()
    {
        GameObject currentAnchorContainer = getCurrentAnchorGameObject();
        changeAnchorMaterials(currentAnchorContainer, highlightMaterial);
    }

    public string getCurrentAnchorID()
    {
        return currentCloudAnchor.Identifier;
    }

    public string getCurrentAnchorAttributes()
    {
        IDictionary<string, string> attributes = currentCloudAnchor.AppProperties;
        return  "" + string.Join(" \n ", attributes.Select(kv => char.ToUpper(kv.Key[0]) + kv.Key.Substring(1) + " : " + kv.Value).ToArray()) + "";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
