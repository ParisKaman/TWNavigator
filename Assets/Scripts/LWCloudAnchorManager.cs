using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors.Unity.Android;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LWCloudAnchorManager : MonoBehaviour
{

    private CloudSpatialAnchorSession cloudSession = null;

    [SerializeField]
    private ARSession aRSession;

    [SerializeField]
    private ARCameraManager aRCameraManager;

    private bool sessionIsStarted = false;
    protected long lastFrameProcessedTimeStamp;

#if UNITY_ANDROID
    private static bool javaInitialized = false; // We should only run the Java initialization once
#endif

    public Text debugText;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Manager started");
        startSessionAsync();
        createSessionAsync();
    }

    private async Task startSessionAsync()
    {
        if (sessionIsStarted)
        {
            //session already started
            return;
        }

        if (cloudSession == null)
        {
            //session already created
            await createSessionAsync();
        }

        cloudSession.Start();

        debugText.text += "\nSession Started";
        debugText.text += "\nSession Pointer: " + cloudSession.Session;
        debugText.text += "\nSession Id: " + cloudSession.SessionId;

        sessionIsStarted = true;
    }

    private async Task createSessionAsync()
    {
        if(cloudSession != null)
        {
            //session already created, need to exit
            return;
        }



        cloudSession = new CloudSpatialAnchorSession();
        cloudSession.Configuration.AccountId = "87f0e62e-2379-4bc4-bd1e-38dc699e8d6b";
        cloudSession.Configuration.AccessToken = "kF/SejjecfM8CatFa74V94iAsgApvPiaT1mY//qgK44=";
        cloudSession.Session = aRSession.subsystem.nativePtr.GetPlatformPointer();


#if UNITY_ANDROID // Android Only
        // We should only run the Java initialization once
        if (!javaInitialized)
        {
            // Create a TaskCompletionSource that we can use to know when
            // the Java plugin has completed initialization on the Android
            // thread.
            TaskCompletionSource<bool> pluginInit = new TaskCompletionSource<bool>();

            // Make sure ARCore is running. This code must be executed
            // on a Java thread provided by Android.
            AndroidHelper.Instance.DispatchUiThread(unityActivity =>
            {
                // Create the plugin
                using (AndroidJavaClass cloudServices = new AndroidJavaClass("com.microsoft.CloudServices"))
                {
                    // Initialize the plugin
                    cloudServices.CallStatic("initialize", unityActivity);

                    // Update static variable to say that the plugin has been initialized
                    javaInitialized = true;

                    // Set the task completion source so the CreateSession method can
                    // continue back on the Unity thread.
                    pluginInit.SetResult(true);
                }
            });

            // Wait for the plugin to complete initialization on the
            // Java thread.
            await pluginInit.Task;
        }
#endif


#if UNITY_ANDROID || UNITY_IOS
        var cameraParams = new XRCameraParams
        {
            zNear = Camera.main.nearClipPlane,
            zFar = Camera.main.farClipPlane,
            screenWidth = Screen.width,
            screenHeight = Screen.height,
            screenOrientation = Screen.orientation
        };

        XRCameraFrame xRCameraFrame;
        if (aRCameraManager.subsystem.TryGetLatestFrame(cameraParams, out xRCameraFrame))
        {
            long latestFrameTimeStamp = xRCameraFrame.timestampNs;

            bool newFrameToProcess = latestFrameTimeStamp > lastFrameProcessedTimeStamp;

            if (newFrameToProcess)
            {
                cloudSession.ProcessFrame(xRCameraFrame.nativePtr.GetPlatformPointer());
                lastFrameProcessedTimeStamp = latestFrameTimeStamp;
            }
        }
#endif

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
