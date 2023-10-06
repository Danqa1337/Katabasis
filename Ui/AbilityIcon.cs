using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;

public class AbilityIcon : KatabasisSelectable
{
    public Label CostGnosisLabel;
    public AbilityData abilityData;
    public UiFlicker flicker;

    public static event Action<AbilityData> OnPointerEnterAbility;

    public static event Action OnPointerExitAbility;

    private void Awake()
    {
        flicker = GetComponent<UiFlicker>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterAbility?.Invoke(abilityData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitAbility?.Invoke();
    }

    public UnityEvent OnPointerHovering;
    public UnityEvent OnPointerStopHovering;
}