using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class VisualizationManager : MonoBehaviour
{
    [SerializeField]
    private ARPlaneManager arPlaneManager;
    private GameObject planePrefab;
    private LineRenderer planeLineRenderer;
    private ARPlaneMeshVisualizer planeMeshVizualizer;

    [SerializeField]
    private ARPointCloudManager arPointCloudManager;
    private GameObject pointCloudPrefab;
    private ARPointCloudParticleVisualizer pointCloudVisualizer;

    // Start is called before the first frame update
    void Start()
    {
        planePrefab = arPlaneManager.planePrefab;
        pointCloudPrefab = arPointCloudManager.pointCloudPrefab;
        planeLineRenderer = planePrefab.GetComponent<LineRenderer>();
        planeMeshVizualizer = planePrefab.GetComponent<ARPlaneMeshVisualizer>();
        pointCloudVisualizer = pointCloudPrefab.GetComponent<ARPointCloudParticleVisualizer>();
        DisablePlanes();
        DisablePointCloud();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DisablePlanes()
    {
        planeLineRenderer.enabled = false;
        planeMeshVizualizer.enabled = false;
    }

    private void DisablePointCloud()
    {
        pointCloudVisualizer.enabled = false;
        Debug.Log("Point Cloud Visualizer = false");
        //arPointCloudManager.enabled = false;
    }

    private void TogglePlanes()
    {
        planeLineRenderer.enabled = !planeLineRenderer.enabled;
        planeMeshVizualizer.enabled = !planeMeshVizualizer.enabled;
        SetAllPlanesActive(planeMeshVizualizer.enabled);
    }

    private void TogglePointCloud()
    {
        pointCloudVisualizer.enabled = !pointCloudVisualizer.enabled;
        Debug.Log("Point Cloud Visualizer = " + pointCloudVisualizer.enabled);
        //arPointCloudManager.enabled = !arPointCloudManager.enabled;
    }

    private void SetAllPlanesActive(bool value)
    {
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.GetComponent<ARPlaneMeshVisualizer>().enabled = value;
        }
    }

    public void ToggleAllVisualizers()
    {
        //TogglePointCloud();
        TogglePlanes();
    }
}
