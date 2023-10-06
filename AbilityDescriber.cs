using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using System.ComponentModel;
using UnityEngine.UI;

[RequireComponent(typeof(TooltipCanvas))]
[Binding]
public class AbilityDescriber : TooltipDescriber, INotifyPropertyChanged
{
    private int _gnosisCost;
    private string _timeCost;
    private string _targeting;
    private string _name;
    private string _description;

    [Binding]
    public int GnosisCost
    {
        get => _gnosisCost;
        set
        {
            _gnosisCost = value;
            InvokePropertyChange("GnosisCost");
        }
    }

    [Binding]
    public string TimeCost
    {
        get => _timeCost;
        set
        {
            _timeCost = value;
            InvokePropertyChange("TimeCost");
        }
    }

    [Binding]
    public string Targeting
    {
        get => _targeting;

        set
        {
            _targeting = value;
            InvokePropertyChange("Targeting");
        }
    }

    [Binding]
    public string Name
    {
        get => _name;

        set
        {
            _name = value;
            InvokePropertyChange("Name");
        }
    }

    [Binding]
    public string Description
    {
        get => _description;

        set
        {
            _description = value;
            InvokePropertyChange("Description");
        }
    }

    private static ContentSizeFitter[] _contentFilters;

    private void OnEnable()
    {
        AbilityIcon.OnPointerEnterAbility += Describe;
        AbilityIcon.OnPointerExitAbility += Hide;
    }

    private void OnDisable()
    {
        AbilityIcon.OnPointerEnterAbility -= Describe;
        AbilityIcon.OnPointerExitAbility -= Hide;
    }

    public void Describe(AbilityData abilityData)
    {
        GnosisCost = abilityData.GnosisCost;
        TimeCost = abilityData.BaseCooldown != 0 ? abilityData.BaseCooldown.ToString() : LocalizationManager.GetString("UnknownTimeCost");
        Targeting = LocalizationManager.GetString("TargetingTypes", abilityData.AbilityTargeting.ToString());
        Name = LocalizationManager.GetString("AbilitiesNames", abilityData.AbilityName.ToString());
        Description = LocalizationManager.GetString("AbilitiesDescriptions", abilityData.AbilityName.ToString());
        Show();
    }
}