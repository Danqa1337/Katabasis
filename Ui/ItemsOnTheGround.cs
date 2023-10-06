using Unity.Entities;
using System;
using UnityEngine;

public class ItemsOnTheGround : Singleton<ItemsOnTheGround>
{
    public static event Action<Inventory> OnItemsChanged;

    private void OnEnable()
    {
        InventorySystem.OnPlayerInventoryChanged += UpdateItems;
        TimeController.OnTurnEnd += UpdateItems;
    }

    private void OnDisable()
    {
        InventorySystem.OnPlayerInventoryChanged += UpdateItems;
        TimeController.OnTurnEnd -= UpdateItems;
    }

    public static void DropItem(Entity entity)
    {
        if (Player.InventoryComponent.Contains(entity))
        {
            PlayersInventory.RemoveItem(entity);
        }
        Player.CurrentTile.Drop(entity);
        ManualSystemUpdater.Update<MovementSystem>();
        UpdateItems();
    }

    public static void Remove(Entity entity)
    {
        Player.CurrentTile.Remove(entity);
        UpdateItems();
    }

    public static void UpdateItems()
    {
        var inventory = new Inventory(LocalizationManager.GetString("GroundContainerName"), Player.CurrentTile.DropLayer);
        OnItemsChanged?.Invoke(inventory);
    }
}