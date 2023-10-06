using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityWeld.Binding;

[Binding]
public class ContainerScreen : InventoryInterface
{
    private void OnEnable()
    {
        TimeController.OnTurnStart += Hide;
        ContainerOpener.OnContainerChanged += Redraw;
    }

    protected void OnDisable()
    {
        TimeController.OnTurnStart -= Hide;
        ContainerOpener.OnContainerChanged -= Redraw;
    }

    private void Hide()
    {
        if (UiManager.IsUIOpened(UIName.Container))
        {
            UiManager.HideUiCanvas(UIName.Container);
        }
    }

    protected override void PlaceItem(Entity entity)
    {
        ContainerOpener.PlaceItem(entity);
    }

    public override void Remove(Entity entity)
    {
        ContainerOpener.RemoveItem(entity);
    }

    protected override void Redraw(Inventory inventory)
    {
        if (UiManager.IsUIOpened(UIName.Container))
        {
            base.Redraw(inventory);
        }
    }

    protected override void OnDubbleClickOnItem(IItemDonor itemDonor)
    {
        var item = itemDonor.TakeItem();
        PlayersInventory.PlaceItem(item);
    }
}

public struct TileContainerTag : IComponentData
{
}

public class Inventory
{
    public readonly string Name;
    public readonly Entity[] Items;

    public Inventory(string name, Entity[] items)
    {
        Name = name;
        Items = items;
    }
}