using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonColorSynchronizer : UnityEngine.UI.Button
{

    TMP_Text text;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color color;
        switch (state)
        {
            case Selectable.SelectionState.Normal:
                color = this.colors.normalColor;
                break;
            case Selectable.SelectionState.Highlighted:
                color = this.colors.highlightedColor;
                break;
            case Selectable.SelectionState.Pressed:
                color = this.colors.pressedColor;
                break;
            case Selectable.SelectionState.Disabled:
                color = this.colors.disabledColor;
                break;
            case Selectable.SelectionState.Selected:
                color = this.colors.selectedColor;
                break;
            default:
                color = Color.black;
                break;
        }
        if (base.gameObject.activeInHierarchy)
        {
            switch (this.transition)
            {
                case Selectable.Transition.ColorTint:
                    ColorTween(color * this.colors.colorMultiplier);
                    break;
            }
        }
    }

    private void ColorTween(Color targetColor)
    {
        if (this.targetGraphic == null)
        {
            this.targetGraphic = this.image;
        }
        if (text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
        }

        base.image.color = targetColor;
        text.color = targetColor;

    }
}
