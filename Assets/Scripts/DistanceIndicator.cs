using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DistanceIndicator : MonoBehaviour
{
    public TextMeshPro indicator;
    public GameObject container;

    NavManager navManager;

    // Start is called before the first frame update
    void Start()
    {
        navManager = FindObjectOfType<NavManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToWaypoint = Vector3.Distance(navManager.destinationLocation,Camera.main.transform.position);
        indicator.text = System.Math.Round(distanceToWaypoint, 1) + "m";
        GameObject textObject = indicator.gameObject;
    }
}
