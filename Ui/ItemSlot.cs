using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Entities;
using System;
using static UnityEditor.Progress;

public abstract class ItemSlot : ItemDisplayingSelectable, IItemDonor, IItemReciever, IBeginDragHandler, IDropHandler, IPointerExitHandler
{
    public static event Action<Entity> OnPointerEnterEvent;

    public static event Action OnPointerExitEvent;

    public Entity Item { get; private set; }

    public void DrawItem(Entity entity)
    {
        if (entity != Entity.Null)
        {
            Item = entity;
            Draw(entity);
        }
        else
        {
            throw new NullReferenceException();
        }
    }

    public override void Clear()
    {
        Item = Entity.Null;
        base.Clear();
    }

    protected override void OnLeftClick()
    {
        base.OnLeftClick();
        FastInventoryActions.Hide();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (Item != Entity.Null)
        {
            OnPointerEnterEvent?.Invoke(Item);
        }
    }

    protected override void OnDubbleClick()
    {
        base.OnDubbleClick();
        OnPointerExit(new PointerEventData(EventSystem.current));
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitEvent?.Invoke();
    }

    protected override void OnRightClick()
    {
        base.OnRightClick();
        FastInventoryActions.Show(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Item != Entity.Null)
        {
            Coursor.StartDrag(this);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        RecieveItem(Coursor.EndDrag());
    }

    public abstract Entity TakeItem();

    public abstract void RecieveItem(IItemDonor donor);
}