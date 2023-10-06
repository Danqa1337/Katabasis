using Unity.Entities;
using UnityWeld.Binding;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Binding]
public class InventoryDescriber : TooltipDescriber
{
    private string _name;
    private string _description;
    private string _scalingSTR;
    private string _scalingAGL;
    private string _attackCost;
    private string _resistance;
    private string _damage;
    private Canvas _canvas;

    private (int, int) _durability;

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
    public string ScalingSTR
    {
        get
        {
            return _scalingSTR;
        }
        set
        {
            _scalingSTR = value;
            InvokePropertyChange("ScalingSTR");
        }
    }

    [Binding]
    public string ScalingAGL
    {
        get
        {
            return _scalingAGL;
        }
        set
        {
            _scalingAGL = value;
            InvokePropertyChange("ScalingAGL");
        }
    }

    [Binding]
    public string AttackCost
    {
        get
        {
            return _attackCost;
        }
        set
        {
            _attackCost = value;
            InvokePropertyChange("AttackCost");
        }
    }

    [Binding]
    public string Damage
    {
        get
        {
            return _damage;
        }
        set
        {
            _damage = value;
            InvokePropertyChange("Damage");
        }
    }

    [Binding]
    public string Resistance
    {
        get
        {
            return _resistance;
        }
        set
        {
            _resistance = value;
            InvokePropertyChange("Resistance");
        }
    }

    [Binding]
    public (int, int) Durability
    {
        get
        {
            return _durability;
        }
        set
        {
            _durability = value;
            InvokePropertyChange("Durability");
        }
    }

    private void OnEnable()
    {
        ItemSlot.OnPointerEnterEvent += Describe;
        ItemSlot.OnPointerExitEvent += Hide;
    }

    private void OnDisable()
    {
        ItemSlot.OnPointerEnterEvent -= Describe;
        ItemSlot.OnPointerExitEvent -= Hide;
    }

    public void Describe(Entity entity)
    {
        var physics = entity.GetComponentData<PhysicsComponent>();
        var durability = entity.GetComponentData<DurabilityComponent>();

        Damage = physics.damage + " + " + StatsCalculator.CalculateBonusDamage(Player.PlayerEntity, entity);
        AttackCost = physics.baseAtackCost.ToString();
        ScalingSTR = physics.ScalingSTR.ToString();
        ScalingAGL = physics.ScalingAGL.ToString();
        Resistance = physics.resistance.ToString();

        var descr = DescriberUtility.GetDescription(entity);

        if (Player.PlayerEntity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull().Contains(entity))
        {
            Description = LocalizationManager.GetString("PlayerBodypartDescription");
        }
        else
        {
            Description = descr;
        }

        Name = DescriberUtility.GetName(entity);
        Durability = (durability.currentDurability, durability.MaxDurability);

        Show();
    }
}