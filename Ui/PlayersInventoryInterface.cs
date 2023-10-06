using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityWeld.Binding;
using static UnityEditor.Progress;

[Binding]
public class PlayersInventoryInterface : InventoryInterface
{
    private void OnEnable()
    {
        PlayersInventory.OnInventoryChanged += Redraw;
        UiManager.OnShowCanvas += OnShow;
    }

    private void OnDisable()
    {
        PlayersInventory.OnInventoryChanged -= Redraw;
        UiManager.OnShowCanvas -= OnShow;
    }

    protected override void PlaceItem(Entity entity)
    {
        PlayersInventory.PlaceItem(entity);
    }

    public override void Remove(Entity entity)
    {
        Debug.Log("remove");
        PlayersInventory.RemoveItem(entity);
    }

    private void OnShow(UIName uIName)
    {
        if (uIName == UIName.Inventory)
        {
            Redraw(PlayersInventory.GetInventory());
        }
    }

    protected override void Redraw(Inventory inventory)
    {
        if (UiManager.IsUIOpened(UIName.Inventory))
        {
            base.Redraw(inventory);
        }
    }

    protected override void OnDubbleClickOnItem(IItemDonor itemDonor)
    {
        var item = itemDonor.TakeItem();
        if (UiManager.IsUIOpened(UIName.Container))
        {
            ContainerOpener.PlaceItem(item);
        }
        else
        {
            if (UiManager.IsUIOpened(UIName.ItemsOnTheGround))
            {
                ItemsOnTheGround.DropItem(item);
            }
            else
            {
                PlayersEquip.PlaceItem(item);
            }
        }
    }
}