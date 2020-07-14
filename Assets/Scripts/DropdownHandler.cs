using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.SpatialAnchors;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownHandler : MonoBehaviour
{
    List<string> waypointNames = new List<string>();

    public List<TMP_Dropdown> dropdownList = new List<TMP_Dropdown>();

    public TMP_Dropdown currentDropdown;

    CloudAnchorManager cloudAnchorManager;

    // Start is called before the first frame update
    void Start()
    {
        cloudAnchorManager = GetComponent<CloudAnchorManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void populateList()
    {
        foreach(TMP_Dropdown dropdown in dropdownList)
        {
            waypointNames.Clear();
            dropdown.ClearOptions();
            List<CloudSpatialAnchor> anchors = cloudAnchorManager.foundAnchors;
            foreach (CloudSpatialAnchor csa in anchors)
            {
                if (csa.AppProperties.ContainsKey("label"))
                {
                    if (csa.AppProperties["label"] != null || csa.AppProperties["label"].Equals(""))
                    {
                        waypointNames.Add(csa.AppProperties["label"]);
                    }
                }
                else
                {
                    waypointNames.Add(csa.Identifier);
                }
            }
            waypointNames.Sort();
            dropdown.AddOptions(waypointNames);
        }
    }
}
