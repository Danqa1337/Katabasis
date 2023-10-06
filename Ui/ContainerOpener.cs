using Unity.Entities;
using System;
using UnityEngine;
using System.Linq;

public class ContainerOpener : Singleton<ContainerOpener>
{
    private Entity _openedContainer;

    public static event Action<Inventory> OnContainerChanged;

    public static void PlaceItem(Entity entity)
    {
        Debug.Log("PlaceItem");
        instance._openedContainer.AddBufferElement(new ChangeInventoryElement(entity, true));
        OpenContainer(instance._openedContainer);
    }

    public static void RemoveItem(Entity entity)
    {
        instance._openedContainer.AddBufferElement(new ChangeInventoryElement(entity, false));
        OpenContainer(instance._openedContainer);
    }

    public static void OpenContainer(Entity entity)
    {
        instance._openedContainer = entity;
        var items = entity.GetComponentData<InventoryComponent>().items.ToArray();
        var name = DescriberUtility.GetName(entity);
        var inventory = new Inventory(name, items);
        OnContainerChanged.Invoke(inventory);
    }
}