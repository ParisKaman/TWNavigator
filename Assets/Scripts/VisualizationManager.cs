using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class VisualizationManager : MonoBehaviour
{
    [SerializeField]
    private ARPlaneManager arPlaneManager;
    private GameObject planePrefab;
    private ARPlaneMeshVisualizer planeMeshVizualizer;

    [SerializeField]
    private Material defaultMaterial;

    private float initialMaterialAlpha;

    bool planesEnabled;

    // Start is called before the first frame update
    void Start()
    {
        planePrefab = arPlaneManager.planePrefab;
        planeMeshVizualizer = planePrefab.GetComponent<ARPlaneMeshVisualizer>();
        initialMaterialAlpha = (defaultMaterial.color.a == 0) ? .33f : defaultMaterial.color.a;
        planesEnabled = true;
        EnablePlanes();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void EnablePlanes()
    {
        Color materialColor = defaultMaterial.color;
        materialColor.a = initialMaterialAlpha;
        defaultMaterial.color = materialColor;
    }

    private void TogglePlanes()
    {
        Color materialColor = defaultMaterial.color;
        materialColor.a = planesEnabled ? 0 : initialMaterialAlpha;
        defaultMaterial.color = materialColor;

        //toggle boolean
        planesEnabled = !planesEnabled;
    }

    public void ToggleAllVisualizers()
    {
        TogglePlanes();
    }
}
