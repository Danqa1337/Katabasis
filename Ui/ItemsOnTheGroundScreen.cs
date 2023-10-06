using Unity.Entities;

public class ItemsOnTheGroundScreen : InventoryInterface
{
    private void OnEnable()
    {
        ItemsOnTheGround.OnItemsChanged += Redraw;
    }

    protected void OnDisable()
    {
        ItemsOnTheGround.OnItemsChanged -= Redraw;
    }

    private void Hide()
    {
        UiManager.HideUiCanvas(UIName.Container);
    }

    protected override void PlaceItem(Entity entity)
    {
        ItemsOnTheGround.DropItem(entity);
    }

    public override void Remove(Entity entity)
    {
        if (!Player.InventoryComponent.Contains(entity))
        {
            Player.PlayerEntity.AddBufferElement(new ChangeInventoryElement(entity, true));
            ManualSystemUpdater.Update<InventorySystem>();
        }
    }

    protected override void Redraw(Inventory inventory)
    {
        if (UiManager.IsUIOpened(UIName.ItemsOnTheGround))
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