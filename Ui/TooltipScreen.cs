using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

public class TooltipScreen : MonoBehaviour
{
    [SerializeField] private Canvas _parentCanvas;
    [SerializeField] private RectTransform _activeHolder;
    [SerializeField] private RectTransform _disabledHolder;
    [SerializeField] private float _defaultOffset;
    private ContentSizeFitter _contentSizeFilter;

    private void Start()
    {
        _contentSizeFilter = _activeHolder.GetComponent<ContentSizeFitter>();
        Clear();
    }

    private void Update()
    {
        var mousePos = Mouse.current.position.ReadValue();
        var childRect = _activeHolder.rect;
        var screenRect = _parentCanvas.pixelRect;

        var offsetX = _defaultOffset;
        var offsetY = _defaultOffset;

        var overRightBorder = mousePos.x + _defaultOffset + childRect.width > screenRect.width;
        var overTopBorder = mousePos.y + _defaultOffset + childRect.height > screenRect.height;

        if (overRightBorder)
        {
            offsetX = (_defaultOffset + childRect.width) * -1;
        }

        if (overTopBorder)
        {
            offsetY = (_defaultOffset + childRect.height) * -1;
        }

        _activeHolder.anchoredPosition = mousePos + new Vector2(offsetX, offsetY);
    }

    public void Attach(TooltipCanvas tooltipCanvas)
    {
        Clear();
        tooltipCanvas.transform.SetParent(_activeHolder);
        ForceRebuildLayouts();
    }

    public void Clear()
    {
        foreach (var item in _activeHolder.GetComponentsInChildren<TooltipCanvas>())
        {
            item.transform.SetParent(_disabledHolder);
            item.transform.localPosition = Vector3.zero;
        }
    }

    private void ForceRebuildLayouts()
    {
        foreach (var item in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }
        _contentSizeFilter.SetLayoutVertical();
        _contentSizeFilter.SetLayoutHorizontal();
    }
}