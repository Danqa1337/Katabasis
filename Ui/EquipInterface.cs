using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Entities;

public class EquipInterface : MonoBehaviour
{
    [SerializeField] private EquipSlot _itemInMainHandSlot;
    [SerializeField] private EquipSlot _itemInOffHandSlot;
    [SerializeField] private EquipSlot _itemOnHeadSlot;
    [SerializeField] private EquipSlot _itemOnChestSlot;

    private List<EquipSlot> EquipSlots => new List<EquipSlot>() { _itemInMainHandSlot, _itemInOffHandSlot, _itemOnChestSlot, _itemOnHeadSlot };

    private void OnEnable()
    {
        UiManager.OnShowCanvas += OnOpenedCanvas;
        EquipmentSystem.OnPlayerEquipChanged += Redraw;
        PlayersAnatomy.OnAnatomyChaged += Redraw;
        foreach (var item in EquipSlots)
        {
            item.OnItemPlaced += OnItemPlaced;
            item.OnItemRemoved += OnItemRemoved;
        }
    }

    private void OnDisable()
    {
        UiManager.OnShowCanvas -= OnOpenedCanvas;
        EquipmentSystem.OnPlayerEquipChanged -= Redraw;
        PlayersAnatomy.OnAnatomyChaged -= Redraw;
        foreach (var item in EquipSlots)
        {
            item.OnItemPlaced -= OnItemPlaced;
            item.OnItemRemoved -= OnItemRemoved;
        }
    }

    private void OnOpenedCanvas(UIName uIName)
    {
        if (uIName == UIName.Inventory)
        {
            Redraw();
        }
    }

    private void OnItemPlaced(Entity entity, EquipTag equipTag)
    {
        PlayersEquip.PlaceItem(entity, equipTag);
    }

    private void OnItemRemoved(EquipTag equipTag)
    {
        PlayersEquip.RemoveItem(equipTag);
    }

    private void Redraw()
    {
        if (UiManager.IsUIOpened(UIName.Inventory))
        {
            foreach (var slot in EquipSlots)
            {
                slot.Clear();
                if (PlayersAnatomy.CanHold(slot.EquipTag))
                {
                    slot.Show();
                    var entity = PlayersEquip.GetEntity(slot.EquipTag);
                    if (entity != Entity.Null)
                    {
                        slot.DrawItem(entity);
                    }
                }
                else
                {
                    slot.Hide();
                }
            }
        }
    }
}