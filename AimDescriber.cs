using Unity.Entities;
using UnityWeld.Binding;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Binding]
public class AimDescriber : TooltipDescriber, INotifyPropertyChanged
{
    private string _name;
    private string _description;

    private int _currentDurability;
    private int _maxDurability;

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

    [Binding]
    public int CurrentDurability
    {
        get
        {
            return _currentDurability;
        }
        set
        {
            _currentDurability = value;
            InvokePropertyChange("CurrentDurability");
        }
    }

    [Binding]
    public int MaxDurability
    {
        get
        {
            return _maxDurability;
        }
        set
        {
            _maxDurability = value;
            InvokePropertyChange("MaxDurability");
        }
    }

    private void OnEnable()
    {
        AimScreen.OnPointerEnterEvent += Describe;
        AimScreen.OnPointerExitEvent += Hide;
    }

    private void OnDisable()
    {
        AimScreen.OnPointerEnterEvent -= Describe;
        AimScreen.OnPointerExitEvent -= Hide;
    }

    public void Describe(Entity entity, bool isAlly)
    {
        var durability = entity.GetComponentData<DurabilityComponent>();

        if (entity.HasComponent<BodyPartComponent>())
        {
            Name = LocalizationManager.GetString("Bodyparts", entity.GetComponentData<BodyPartComponent>().bodyPartTag.ToString());
        }
        else
        {
            Name = LocalizationManager.GetString("Bodyparts", "Body");
        }
        Description = Name;
        CurrentDurability = durability.currentDurability;
        MaxDurability = durability.MaxDurability;

        Show();
    }
}