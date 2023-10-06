using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityWeld.Binding;
using System.ComponentModel;

using UnityEngine;

using UnityEngine.UI;
using Locations;

[Binding]
public class TransitionDescriber : TooltipDescriber, INotifyPropertyChanged
{
    private string _description;

    [Binding]
    public string Description
    {
        get
        {
            return _description;
        }
        set
        {
            _description = value;
            InvokePropertyChange("Description");
        }
    }

    private void OnEnable()
    {
        TransitionIcon.OnPointerEnterEvent += Describe;
        TransitionIcon.OnPointerExitEvent += Hide;
    }

    private void OnDisable()
    {
        TransitionIcon.OnPointerEnterEvent -= Describe;
        TransitionIcon.OnPointerExitEvent -= Hide;
    }

    public void Describe(Transition locationTransition)
    {
        var descriptionText = LocalizationManager.GetString("TransitionDescriptionStart");
        var directionDescription = LocalizationManager.GetString("TransitionDirectionDescription");

        if (locationTransition.EntranceLocation.IsCurrentLocation)
        {
            if (locationTransition.ExitLocation.IsGenerated)
            {
                descriptionText += "\n" + directionDescription + locationTransition.ExitLocation.Name;
            }
            else
            {
                descriptionText += "\n" + LocalizationManager.GetString("UnknownLocationTransitionDescription");
            }
        }
        else
        {
            if (locationTransition.EntranceLocation.IsGenerated)
            {
                descriptionText += "\n" + directionDescription + locationTransition.EntranceLocation.Name;
            }
            else
            {
                descriptionText += "\n" + LocalizationManager.GetString("UnknownLocationTransitionDescription");
            }
        }
        descriptionText += "\n" + LocalizationManager.GetString("TransitionDescriptionEnd");
        Description = descriptionText;

        foreach (var item in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }

        Show();
    }
}