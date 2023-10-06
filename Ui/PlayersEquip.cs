using Unity.Entities;

public class PlayersEquip
{
    public static Entity GetEntity(EquipTag equipTag)
    {
        return Player.EquipmentComponent.GetEquipmentEntity(equipTag);
    }

    public static void PlaceItem(Entity entity)
    {
        PlaceItem(entity, EquipTag.Weapon);
    }

    public static void PlaceItem(Entity entity, EquipTag equipTag)
    {
        if (PlayersAnatomy.CanHold(equipTag))
        {
            if (PlayersInventory.HasItem(entity))
            {
                PlayersInventory.RemoveItem(entity);
            }
            var existingEntity = Player.EquipmentComponent.GetEquipmentEntity(equipTag);
            if (existingEntity != Entity.Null)
            {
                RemoveItem(equipTag);
            }
            Player.PlayerEntity.AddBufferElement(new ChangeEquipmentElement(entity, equipTag));
            ManualSystemUpdater.Update<EquipmentSystem>();
        }
    }

    public static void RemoveItem(EquipTag equipTag)
    {
        var entity = Player.EquipmentComponent.GetEquipmentEntity(equipTag);
        if (entity != Entity.Null)
        {
            Player.PlayerEntity.AddBufferElement(new ChangeEquipmentElement(Entity.Null, equipTag));
            ManualSystemUpdater.Update<EquipmentSystem>();
            PlayersInventory.PlaceItem(entity);
        }
    }
}