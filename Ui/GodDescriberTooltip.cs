using Gods;
using System.Collections;
using UnityEngine;

public class GodDescriberTooltip : TooltipDescriber
{
    [SerializeField] private Label _descriptionLabel;
    [SerializeField] private Label _nameLabel;
    [SerializeField] private Label _relationsLabel;
    [SerializeField] private Label _attentionLabel;

    private void OnEnable()
    {
        GodIcon.OnPointerEnterStatic += Describe;
        GodIcon.OnPointerExitStatic += Hide;
    }

    private void OnDisable()
    {
        GodIcon.OnPointerEnterStatic -= Describe;
        GodIcon.OnPointerExitStatic -= Hide;
    }

    public void Describe(int index)
    {
        var god = Registers.GodsRegister.GetGod(index);
        var GodArchetype = god.GodArchetype;
        _descriptionLabel.SetText(LocalizationManager.GetString("GodsDescriptions", GodArchetype.ToString()));
        _nameLabel.SetText(god.Name);
        _relationsLabel.SetText(god.Relations.ToString());
        _attentionLabel.SetText(LocalizationManager.GetString("AttentionLevels", god.AttentionLevel.ToString()));

        Show();
    }
}