using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{

    public GameObject MenuPanel;
    public GameObject BlurPanel;

    public BlurPanelHandler blurPanelHandler;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HideMenuPanel()
    {
        if(blurPanelHandler != null)
        {
          blurPanelHandler.HideBlurPanel();
        }

        if (MenuPanel != null)
        {
            Animator animator = MenuPanel.GetComponent<Animator>();
            if (animator != null)
            {
                bool isOpen = animator.GetBool("open");
                animator.SetBool("open", false);
            }
        }
    }

    public void ToggleMenuPanel()
    {
        if(blurPanelHandler != null)
        {
          blurPanelHandler.ToggleBlurPanel();
        }

        if(MenuPanel != null)
        {
            Animator animator = MenuPanel.GetComponent<Animator>();
            if(animator != null)
            {
                bool isOpen = animator.GetBool("open");
                animator.SetBool("open", !isOpen);
            }
        }
    }
}
