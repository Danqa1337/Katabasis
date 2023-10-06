using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

public enum UIName
{
    Null,
    Inventory,
    Pause,
    Console,
    Describer,
    Death,
    Log,
    CharacterSellection,
    Map,
    Settings,
    Controlls,
    RadialMenu,
    Aiming,
    Anatomy,
    Container,
    Perks,
    ItemsOnTheGround,
    Gods,
}

[Binding]
public class UiManager : Singleton<UiManager>
{
    private List<UIName> _openCanvasList;
    private LayoutGroup _layoutGroup;
    public static Action<UIName> OnShowCanvas;
    public static Action<UIName> OnHideCanvas;
    [SerializeField] private bool _debug;

    private void OnEnable()
    {
        Controller.OnControllerActionInvoked += ListenInput;
    }

    private void OnDisable()
    {
        Controller.OnControllerActionInvoked -= ListenInput;
    }

    private void ListenInput(InputContext inputContext)
    {
        if (inputContext.Action == ControllerActionName.CloseLast)
        {
            CloseLast();
        }
        if (inputContext.Action == ControllerActionName.ToggleConsole)
        {
            ToggleUiCanvas(UIName.Console);
        }
        if (inputContext.Action == ControllerActionName.ToggleInventory)
        {
            ToggleUiCanvas(UIName.Inventory);
        }
        if (inputContext.Action == ControllerActionName.ToggleDescriber)
        {
            ToggleUiCanvas(UIName.Describer);
        }
        if (inputContext.Action == ControllerActionName.ToggleMap)
        {
            ToggleUiCanvas(UIName.Map);
        }
        if (inputContext.Action == ControllerActionName.ShowActions)
        {
            ShowUiCanvas(UIName.RadialMenu);
        }
        if (inputContext.Action == ControllerActionName.HideActions)
        {
            HideUiCanvas(UIName.RadialMenu);
        }
    }

    private void Awake()
    {
        _layoutGroup = GetComponent<LayoutGroup>();
        _openCanvasList = new List<UIName>();
    }

    public static bool IsUIOpened(UIName uIName)
    {
        return instance._openCanvasList.Contains(uIName);
    }

    private void UpdatePositions()
    {
        _layoutGroup.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.transform as RectTransform);
    }

    public static void HideUiCanvas(UIName uIName)
    {
        if (instance._debug) Debug.Log("Hiding " + uIName);
        if (instance._openCanvasList.Contains(uIName))
        {
            instance._openCanvasList.Remove(uIName);
        }

        if (instance._openCanvasList.Count == 0)
        {
            Controller.ChangeActionMap(ActionMap.Default);
        }
        else
        {
            var lastCanvas = instance._openCanvasList.Last();
            Controller.ChangeActionMap(GetActionMapFromUiCanvasName(lastCanvas));
        }

        OnHideCanvas?.Invoke(uIName);
        instance.UpdatePositions();
    }

    public static void CloseLast()
    {
        if (instance._openCanvasList.Count > 0)
        {
            var last = instance._openCanvasList.Last();
            HideUiCanvas(last);
        }
        else
        {
            ShowUiCanvas(UIName.Pause);
        }
    }

    public static void ShowUiCanvas(UIName uiName)
    {
        if (instance._debug) Debug.Log("Showing " + uiName);

        if (!instance._openCanvasList.Contains(uiName))
        {
            instance._openCanvasList.Remove(uiName);
        }
        instance._openCanvasList.Add(uiName);

        Controller.ChangeActionMap(GetActionMapFromUiCanvasName(uiName));
        OnShowCanvas?.Invoke(uiName);
        instance.UpdatePositions();
    }

    public static void ToggleUiCanvas(UIName uIName)
    {
        if (IsUIOpened(uIName))
        {
            HideUiCanvas(uIName);
        }
        else
        {
            ShowUiCanvas(uIName);
        }
    }

    [Binding]
    public void CloseAll()
    {
        for (int i = 0; i < _openCanvasList.Count; i++)
        {
            CloseLast();
        }
    }

    private static ActionMap GetActionMapFromUiCanvasName(UIName uiCanvasName) => (uiCanvasName) switch
    {
        UIName.Console => ActionMap.Console,
        UIName.Inventory => ActionMap.Default,
        UIName.Container => ActionMap.Default,
        UIName.ItemsOnTheGround => ActionMap.Default,
        UIName.RadialMenu => ActionMap.RadialMenu,
        _ => ActionMap.Menuing,
    };
}