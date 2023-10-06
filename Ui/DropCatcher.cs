using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropCatcher : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    public event Action OnItemDroped;

    public event Action OnPointerEnterEvent;

    public void OnDrop(PointerEventData eventData)
    {
        var donor = Coursor.EndDrag();
        if (donor != null)
        {
            var item = donor.TakeItem();
            ItemsOnTheGround.DropItem(item);
            OnItemDroped?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }
}