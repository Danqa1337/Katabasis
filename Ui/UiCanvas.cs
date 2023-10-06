using System;
using UnityEngine;

public abstract class UiCanvas : MonoBehaviour
{
    private Canvas _canavs;

    private Canvas Canvas
    {
        get
        {
            if (_canavs == null) _canavs = GetComponent<Canvas>();
            return _canavs;
        }
    }

    public Action OnShow;
    public Action OnHide;
    public bool IsShown => Canvas.renderMode == RenderMode.ScreenSpaceOverlay;

    public abstract UIName UIName { get; }

    protected virtual void OnEnable()
    {
        UiManager.OnShowCanvas += Show;
        UiManager.OnHideCanvas += Hide;
    }

    protected virtual void OnDisable()
    {
        UiManager.OnShowCanvas -= Show;
        UiManager.OnHideCanvas -= Hide;
    }

    private void Show(UIName uIName)
    {
        if (uIName == UIName)
        {
            Show();
        }
    }

    private void Hide(UIName uIName)
    {
        if (uIName == UIName)
        {
            Hide();
        }
    }

    private void Hide()
    {
        OnHide?.Invoke();
        DoOnCanvasHide();
        Canvas.renderMode = RenderMode.WorldSpace;
    }

    private void Show()
    {
        OnShow?.Invoke();
        DoOnCanvasShow();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    protected virtual void DoOnCanvasShow()
    {
    }

    protected virtual void DoOnCanvasHide()
    {
    }
}