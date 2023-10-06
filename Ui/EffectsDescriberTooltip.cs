using System.Collections;
using UnityEngine;

public class EffectsDescriberTooltip : TooltipDescriber
{
    [SerializeField] private Label _nameLabel;
    [SerializeField] private Label _descriptionLabel;

    private void OnEnable()
    {
        EffectIcon.OnPointerEnterStatic += Describe;
        EffectIcon.OnPointerExitStatic += Hide;
    }

    private void OnDisable()
    {
        EffectIcon.OnPointerEnterStatic -= Describe;
        EffectIcon.OnPointerExitStatic -= Hide;
    }

    public void Describe(EffectName effectName)
    {
        var perk = EffectsDatabase.GetEffect(effectName);

        _descriptionLabel.SetText(LocalizationManager.GetString("EffectsDescriptions", effectName.ToString()));
        _nameLabel.SetText(LocalizationManager.GetString("EffectsNames", effectName.ToString()));

        Show();
    }
}