using Perks;
using System.Collections;
using UnityEngine;

public class PerkDescriberTooltip : TooltipDescriber
{
    [SerializeField] private Label _nameLabel;
    [SerializeField] private Label _descriptionLabel;

    private void OnEnable()
    {
        PerkIcon.OnPointerEnterStatic += Describe;
        PerkIcon.OnPointerExitStatic += Hide;
    }

    private void OnDisable()
    {
        PerkIcon.OnPointerEnterStatic -= Describe;
        PerkIcon.OnPointerExitStatic -= Hide;
    }

    public void Describe(PerkName perkName)
    {
        var perk = PerksDatabase.GetPerk(perkName);
        if (perk is AbilityGrantingPerk)
        {
            var abilityGrantingPerk = perk as AbilityGrantingPerk;
            _descriptionLabel.SetText(LocalizationManager.GetString("AbilitiesDescriptions", abilityGrantingPerk.AbilityName.ToString()));
            _nameLabel.SetText(LocalizationManager.GetString("AbilitiesNames", abilityGrantingPerk.AbilityName.ToString()));
        }
        else
        {
            _descriptionLabel.SetText(LocalizationManager.GetString("PerksDescriptions", perkName.ToString()));
            _nameLabel.SetText(LocalizationManager.GetString("PerksNames", perkName.ToString()));
        }
        Show();
    }
}