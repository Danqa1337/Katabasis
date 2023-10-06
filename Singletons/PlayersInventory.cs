using Unity.Entities;
using System;
using System.Linq;

public class PlayersInventory : Singleton<PlayersInventory>
{
    public static event Action<Inventory> OnInventoryChanged;

    private void OnEnable()
    {
        InventorySystem.OnPlayerInventoryChanged += delegate { OnInventoryChanged?.Invoke(GetInventory()); };
    }

    private void OnDisable()
    {
        InventorySystem.OnPlayerInventoryChanged -= delegate { OnInventoryChanged?.Invoke(GetInventory()); };
    }

    public static Inventory GetInventory()
    {
        var name = DescriberUtility.GetName(Player.PlayerEntity);
        var items = Player.PlayerEntity.GetComponentData<InventoryComponent>().items.ToArray();
        return new Inventory(name, items);
    }

    public static bool HasItem(Entity entity)
    {
        return Player.InventoryComponent.Contains(entity);
    }

    public static void PlaceItem(Entity entity)
    {
        if (entity != Entity.Null)
        {
            if (!Player.InventoryComponent.Contains(entity))
            {
                Player.PlayerEntity.AddBufferElement(new ChangeInventoryElement(entity, true));
                ManualSystemUpdater.Update<InventorySystem>();
            }
            else
            {
                OnInventoryChanged?.Invoke(GetInventory());
            }
        }
    }

    public static void RemoveItem(Entity entity)
    {
        if (Player.InventoryComponent.Contains(entity))
        {
            Player.PlayerEntity.AddBufferElement(new ChangeInventoryElement(entity, false));
            ManualSystemUpdater.Update<InventorySystem>();
        }
    }
}