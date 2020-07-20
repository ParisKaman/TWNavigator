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

    // Start is called before the first frame update
    void Start()
    {
        planePrefab = arPlaneManager.planePrefab;
        planeLineRenderer = planePrefab.GetComponent<LineRenderer>();
        planeMeshVizualizer = planePrefab.GetComponent<ARPlaneMeshVisualizer>();
        DisablePlanes();
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

    private void TogglePlanes()
    {
        planeLineRenderer.enabled = !planeLineRenderer.enabled;
        planeMeshVizualizer.enabled = !planeMeshVizualizer.enabled;
        SetAllPlanesActive(planeMeshVizualizer.enabled);
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
        TogglePlanes();
    }
}
