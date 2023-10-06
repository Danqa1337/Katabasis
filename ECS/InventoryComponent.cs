using System.Collections.Generic;
using Unity.Entities;
public struct InventoryBufferElement : IBufferElementData
{
    public Entity entity;

    public InventoryBufferElement(Entity entity)
    {
        this.entity = entity;
    }
}
public struct InventoryComponent : IComponentData
{
    public Entity parent;
    private DynamicBuffer<InventoryBufferElement> _items => parent.GetBuffer<InventoryBufferElement>();

    public InventoryComponent(Entity parent)
    {
        this.parent = parent;
        parent.AddBuffer<InventoryBufferElement>();
    }

    public HashSet<Entity> items
    {

        get
        {
            var hashset = new HashSet<Entity>();
            foreach (var item in _items)
            {
                hashset.Add(item.entity);
            }
            return hashset;
        }
    }

    public bool Contains(Entity entity)
    {
        return items.Contains(entity);
    }

    public Entity FindItem(SimpleObjectName itemName)
    {
        foreach (var i in items)
        {
            if (i.GetComponentData<SimpleObjectNameComponent>().simpleObjectName == itemName)
            {

                return i;
            }
        }

        return Entity.Null;
    }






}
