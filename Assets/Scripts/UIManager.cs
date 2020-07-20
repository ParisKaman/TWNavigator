using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public string anchorLabel = "";

    [SerializeField]
    private GameObject defaultUICanvas;
    [SerializeField]
    private GameObject labelUICanvas;
    [SerializeField]
    private GameObject deleteUICanvas;
    [SerializeField]
    private GameObject wayfindUICanvas;
    [SerializeField]
    private GameObject destinationReachedCanvas;
    [SerializeField]
    private TMP_InputField labelInput;

    private List<GameObject> canvasList = new List<GameObject>();

    DropdownHandler dropdownHandler = null;
    ARTapToPlace arTapManager = null;
    CloudAnchorManager cloudAnchorManager = null;
    NavManager navManager = null;

    // Start is called before the first frame update
    void Start()
    {
        arTapManager = FindObjectOfType<ARTapToPlace>();
        cloudAnchorManager = FindObjectOfType<CloudAnchorManager>();
        navManager = FindObjectOfType<NavManager>();
        dropdownHandler = FindObjectOfType<DropdownHandler>();
        AddAllCanvases();
        activateDefaultView();
    }

    private void AddAllCanvases()
    {
      canvasList.Add(defaultUICanvas);
      canvasList.Add(labelUICanvas);
      canvasList.Add(deleteUICanvas);
      canvasList.Add(wayfindUICanvas);
      canvasList.Add(destinationReachedCanvas);
    }

    public void activateDefaultView()
    {
      SetActiveCanvas(defaultUICanvas);
    }

    public void SetActiveCanvas(GameObject targetCanvas)
    {
      foreach(GameObject canvas in canvasList){
        if(canvas == targetCanvas)
        {
          canvas.gameObject.SetActive(true);
        }
        else
        {
          canvas.gameObject.SetActive(false);
        }
      }
    }

    public void destinationReachedUI()
    {
        SetActiveCanvas(destinationReachedCanvas);
        string nextStop = navManager.getNextStop();
        destinationReachedCanvas.transform.Find("Panel").Find("Message Container").Find("Next Stop").GetComponent<TMP_Text>().text = nextStop;
    }

    public void confirmDestination()
    {
        SetActiveCanvas(defaultUICanvas);
    }

    public void beginWayfinding()
    {
        SetActiveCanvas(wayfindUICanvas);
        navManager.BeginWayfind(dropdownHandler.dropdownList[0].options[dropdownHandler.dropdownList[0].value].text);
        dropdownHandler.currentDropdown = dropdownHandler.dropdownList[0];
    }

    public void submitAnchor()
    {
        anchorLabel = labelInput.text;
        //place the anchor with the label string
        arTapManager.PlaceAnchor(anchorLabel);

        //hide label UI canvas and return to show default UI canvas
        SetActiveCanvas(defaultUICanvas);
    }

    public void cancelDelete()
    {
        SetActiveCanvas(defaultUICanvas);
        cloudAnchorManager.cancelDelete();
    }

    public void deleteAnchor()
    {
        cloudAnchorManager.deleteCurrentCloudAnchor();
        SetActiveCanvas(defaultUICanvas);
    }

    public void defaultToDelete()
    {
        HideSideMenu();
        SetActiveCanvas(deleteUICanvas);
        displayAnchorToDelete();
        cloudAnchorManager.hoverDelete();
    }

    private void displayAnchorToDelete()
    {
        TMP_Text anchorInformation = deleteUICanvas.transform.Find("Main Panel").Find("Message Container").GetComponentInChildren<TMP_Text>();
        anchorInformation.text = "Would you like to delete this anchor?";
        string anchorAttributes = cloudAnchorManager.getCurrentAnchorAttributes();
        if(anchorAttributes.Length > 1)
        {
          anchorInformation.text += "\n" + anchorAttributes;
        }
        else
        {
          anchorInformation.text += "\nID: " + cloudAnchorManager.getCurrentAnchorID();
        }
    }

    public void defaultToLabel()
    {
        HideSideMenu();
        SetActiveCanvas(labelUICanvas);
        arTapManager.EnableIndicator(true);
    }

    public void defaultToWayfind()
    {
        HideSideMenu();
        SetActiveCanvas(wayfindUICanvas);
        dropdownHandler.populateList();
    }

    private void HideSideMenu()
    {
        MenuHandler menuHandler = FindObjectOfType<MenuHandler>();
        if(menuHandler != null)
        {
            menuHandler.HideMenuPanel();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
