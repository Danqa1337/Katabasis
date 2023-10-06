using System.Collections.Generic;
using Unity.Entities;

public struct TraderComponent : IComponentData
{
    public Entity self;
    public List<TradingItemElement> GetTradingItems()
    {
        var list = new List<TradingItemElement>();
        foreach (var item in self.GetBuffer<TradingItemElement>())
        {
            list.Add(item);
        }
        return list;
    }
}

public struct TradingItemElement : IBufferElementData
{
    public Entity item;
    public TradingItemElement(Entity item, float price)
    {
        this.item = item;
    }
}
