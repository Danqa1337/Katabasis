using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : ItemSlot
{
    [SerializeField] private EquipTag _equipTag;
    [SerializeField] private Sprite _disabledSprite;
    [SerializeField] private Sprite _enabledSprite;
    [SerializeField] private Image _slotIcon;

    private bool _isEnabled;
    public bool IsEnabled { get => _isEnabled; }
    public EquipTag EquipTag { get => _equipTag; }

    public event Action<Entity, EquipTag> OnItemPlaced;

    public event Action<EquipTag> OnItemRemoved;

    public void Show()
    {
        _isEnabled = true;
        interactable = true;
        _slotIcon.sprite = _enabledSprite;
    }

    public void Hide()
    {
        _isEnabled = false;
        interactable = false;
        _slotIcon.sprite = _disabledSprite;
    }

    protected override void Awake()
    {
        base.Awake();
        Show();
    }

    public bool CanHoldItem(Entity entity)
    {
        if (_isEnabled && entity != Entity.Null)
        {
            if (_equipTag != EquipTag.Weapon && _equipTag != EquipTag.Shield)
            {
                return entity.GetComponentData<PhysicsComponent>().defaultEquipTag == _equipTag;
            }
            return true;
        }
        return false;
    }

    public override Entity TakeItem()
    {
        var item = Item;
        OnItemRemoved?.Invoke(_equipTag);
        return item;
    }

    public override void RecieveItem(IItemDonor donor)
    {
        if (donor != null && CanHoldItem(donor.Item))
        {
            var prevItem = Item;
            var newItem = donor.TakeItem();

            if (prevItem != Entity.Null)
            {
                OnItemRemoved?.Invoke(_equipTag);
            }
            OnItemPlaced?.Invoke(newItem, _equipTag);
            if (donor is EquipSlot)
            {
                OnItemPlaced?.Invoke(prevItem, (donor as EquipSlot).EquipTag);
            }
        }
    }

    protected override void OnDubbleClick()
    {
        base.OnDubbleClick();
        TakeItem();
    }
}