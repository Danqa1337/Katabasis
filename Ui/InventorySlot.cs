using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : ItemSlot
{
    public event Action<Entity> OnItemGiven;

    public event Action<IItemDonor> OnItemRecieved;

    public override Entity TakeItem()
    {
        var item = Item;
        Clear();
        OnItemGiven?.Invoke(item);
        return item;
    }

    public override void RecieveItem(IItemDonor donor)
    {
        OnItemRecieved.Invoke(donor);
    }

    public override void Clear()
    {
        base.Clear();
        OnItemGiven = null;
        OnItemRecieved = null;
        OnDubbleClickEvent.RemoveAllListeners();
    }
}

public interface IItemDonor
{
    public abstract Entity Item { get; }

    public abstract Entity TakeItem();
}

public interface IItemReciever
{
    public abstract void RecieveItem(IItemDonor donor);
}