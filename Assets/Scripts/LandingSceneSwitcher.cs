using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingSceneSwitcher : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadAdminScene()
    { 
        LoadScene("AdminScene");
    }

    public void LoadCustomerScene()
    {
        LoadScene("CustomerScene");
    }
}
