using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class TooltipCanvas : MonoBehaviour
{
    private TooltipScreen _tooltipScreen;

    private void Awake()
    {
        _tooltipScreen = GetComponentInParent<TooltipScreen>();
    }

    public void Show()
    {
        _tooltipScreen.Attach(this);
        Rebuild();
    }

    public void Hide()
    {
        _tooltipScreen.Clear();
    }

    public void Rebuild()
    {
        foreach (var item in GetComponentsInChildren<ContentSizeFitter>())
        {
            item.SetLayoutHorizontal();
            item.SetLayoutVertical();
        }
        foreach (var item in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.transform as RectTransform);
        }
    }
}