using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityWeld.Binding;
using System.ComponentModel;

using UnityEngine;

using UnityEngine.UI;

[Binding]
public class MapDescriber : TooltipDescriber, INotifyPropertyChanged
{
    private string _name;
    private string _description;

    [Binding]
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
            InvokePropertyChange("Name");
        }
    }

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
        GlobalMapTile.OnPoiterEnterEvent += Describe;
        GlobalMapTile.OnPoiterExitEvent += Hide;
    }

    private void OnDisable()
    {
        GlobalMapTile.OnPoiterEnterEvent -= Describe;
        GlobalMapTile.OnPoiterExitEvent -= Hide;
    }

    public void Describe(Location location)
    {
        Name = location.Name;
        Description = "...";
        if (location.IsCurrentLocation)
        {
            Description = LocalizationManager.GetString("Current location");
        }
        Show();
    }
}