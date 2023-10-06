using Perks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkIcon : KatabasisSelectable
{
    [SerializeField] private Image _image;

    public static event Action<PerkName> OnPointerEnterStatic;

    public static event Action OnPointerExitStatic;

    private PerkName _perkName;

    public void DrawPerk(PerkName perkName)
    {
        _perkName = perkName;
        Sprite sprite = null;
        var perk = PerksDatabase.GetPerk(perkName);
        if (perk is AbilityGrantingPerk)
        {
            var abilityGrnatingPerk = perk as AbilityGrantingPerk;
            var abilityData = AbilitiesDatabase.GetAbilityData(abilityGrnatingPerk.AbilityName);
            sprite = IconDataBase.GetAbilityIcon(abilityGrnatingPerk.AbilityName);
        }
        else
        {
            sprite = IconDataBase.GetPerkIcon(perkName);
        }
        _image.sprite = sprite;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterStatic?.Invoke(_perkName);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitStatic?.Invoke();
    }
}