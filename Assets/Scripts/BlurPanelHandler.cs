using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurPanelHandler : MonoBehaviour
{
    private Color color;
    private Image blurPanel;

    public float blurSpeed = 2f;
    public float blurStrength = 60f;

    private float blurStep;
    private float blurPercent;

    private bool isBlurPanelActive;

    // Start is called before the first frame update
    void Start()
    {
        blurPanel = GetComponent<Image>();
        color = blurPanel.color;
        color.a = 0f;
        blurPanel.color = color;
        blurPanel.raycastTarget = false;
        blurStep = blurSpeed/100f;
        blurStrength = blurStrength > 100f ? 100 : blurStrength;
        blurPercent = blurStrength/100f;
    }

    // Update is called once per frame
    void Update()
    {
      if(isBlurPanelActive)
      {
        BlurPanel();
      }
      else
      {
        HidePanel();
      }
    }

    private void BlurPanel()
    {
      if(blurPanel.color.a < blurPercent)
      {
        Color _color = blurPanel.color;
        _color.a += blurStep;
        blurPanel.color = _color;
      }
    }

    private void HidePanel()
    {
      if(blurPanel.color.a > 0f)
      {
        Color _color = blurPanel.color;
        _color.a -= blurStep;
        blurPanel.color = _color;
      }
    }

    public void HideBlurPanel()
    {
      isBlurPanelActive = false;
      blurPanel.raycastTarget = false;
    }

    public void ToggleBlurPanel()
    {
      isBlurPanelActive = !isBlurPanelActive;
      blurPanel.raycastTarget = !blurPanel.raycastTarget;
    }


}
