using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class EffectIcon : KatabasisSelectable
{
    [SerializeField] private Label _timerLabel;
    [SerializeField] private Image _iconImage;
    private EffectElement _effectElement;

    public static event Action<EffectName> OnPointerEnterStatic;

    public static event Action OnPointerExitStatic;

    public void UpdateIcon(EffectElement effect)
    {
        this._effectElement = effect;
        var durationString = effect.duration.ToString();
        if (effect.duration == -1)
        {
            durationString = "";
        }
        _iconImage.sprite = IconDataBase.GetEffectIcon(effect.EffectName);
        _timerLabel.SetText(durationString);
        _iconImage.sprite = IconDataBase.GetEffectIcon(effect.EffectName);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterStatic?.Invoke(_effectElement.EffectName);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitStatic?.Invoke();
    }
}