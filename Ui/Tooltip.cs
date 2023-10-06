using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(GraphicRaycaster))]
public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<string> OnPointerEnterEvent;

    public static event Action OnPointerExitEvent;

    public enum TooltipText
    {
        HudPortrait,
        Gnosis,
        Time,
        Experience,
        Squadmate,
        STR,
        AGL,
        ToggleEquip,
        GodsCenter,
        GodsHelp,
    }

    public TooltipText tooltipText;
    private bool _pointerIsOverUi;
    private const int _tooltipDelay = 100;

    public async void OnPointerEnter(PointerEventData eventData)
    {
        _pointerIsOverUi = true;
        await Task.Delay(_tooltipDelay);
        if (_pointerIsOverUi)
        {
            OnPointerEnterEvent?.Invoke(LocalizationManager.GetString("TooltipText", tooltipText.ToString()));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerIsOverUi = false;
        OnPointerExitEvent?.Invoke();
    }
}