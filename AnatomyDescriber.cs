using Unity.Entities;
using UnityWeld.Binding;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Binding]
public class AnatomyDescriber : TooltipDescriber, INotifyPropertyChanged
{
    private string _name;
    private string _description;
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
        AnatomyScreen.OnPointerEnterEvent += Describe;
        AnatomyScreen.OnPointerExitEvent += Hide;
    }

    private void OnDisable()
    {
        AnatomyScreen.OnPointerEnterEvent -= Describe;
        AnatomyScreen.OnPointerExitEvent -= Hide;
    }

    public void Describe(Entity entity)
    {
        var durability = entity.GetComponentData<DurabilityComponent>();
        if (entity.HasComponent<BodyPartComponent>())
        {
            var bodyPart = entity.GetComponentData<BodyPartComponent>().bodyPartTag;
            Name = LocalizationManager.GetString("Bodyparts", bodyPart.ToString());
            if (bodyPart == BodyPartTag.Body || bodyPart == BodyPartTag.Head)
            {
                Description = LocalizationManager.GetString("PlayersBodyOrHeadDescription");
            }
            else
            {
                Description = LocalizationManager.GetString("PlayerBodypartDescription");
            }
        }
        else
        {
            Description = DescriberUtility.GetDescription(entity);
            Name = DescriberUtility.GetName(entity);
        }
        Durability = (durability.currentDurability, durability.MaxDurability);

        Show();
    }
}