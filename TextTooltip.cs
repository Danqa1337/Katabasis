using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTooltip : TooltipDescriber
{
    [SerializeField] private Label _textLabel;

    private void OnEnable()
    {
        Tooltip.OnPointerEnterEvent += SetText;
        Tooltip.OnPointerExitEvent += Hide;
    }

    private void OnDisable()
    {
        Tooltip.OnPointerEnterEvent -= SetText;
        Tooltip.OnPointerExitEvent -= Hide;
    }

    private void SetText(string message)
    {
        _textLabel.SetText(message);
        Show();
    }
}