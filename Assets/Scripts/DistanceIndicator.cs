using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DistanceIndicator : MonoBehaviour
{
    public TextMeshPro indicator;
    public GameObject container;
    //Vector3 baseScale;

    NavManager navManager;

    // Start is called before the first frame update
    void Start()
    {
        navManager = FindObjectOfType<NavManager>();
        //baseScale = container.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToWaypoint = Vector3.Distance(navManager.destinationLocation,Camera.main.transform.position);
        indicator.text = System.Math.Round(distanceToWaypoint, 1) + "m";
        //container.transform.localScale = baseScale * (distanceToWaypoint/10f);
        GameObject textObject = indicator.gameObject;
        //textObject.transform.rotation = Camera.main.transform.rotation;
    }


    //public TextMesh indicator;
    //public GameObject container;
    //Vector3 baseScale;

    //NavigationManager navigationManager;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    navigationManager = FindObjectOfType<NavigationManager>();
    //    baseScale = container.transform.localScale;
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    float distanceToWaypoint = Vector3.Distance(navigationManager.containerLocation,Camera.main.transform.position);
    //    indicator.text = System.Math.Round(distanceToWaypoint, 1) + "m";
    //    container.transform.localScale = baseScale * (distanceToWaypoint/10f);
    //    GameObject textObject = indicator.gameObject;
    //    textObject.transform.rotation = Camera.main.transform.rotation;
    //}
}
