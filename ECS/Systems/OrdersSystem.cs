using Unity.Entities;

public enum OrderType
{
    Null,
    AttackTarget,
    GoTo,
    AtEase,
}

[DisableAutoCreation]
public partial class OrdersSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();
        Entities.ForEach((int entityInQueryIndex, Entity entity, ref OrderComponent orderComponent) =>
        {
            if (orderComponent.lifetime == 0)
            {
                ecb.RemoveComponent<OrderComponent>(entityInQueryIndex, entity);
            }
            else
            {
                orderComponent.lifetime--;
            }
        }).WithBurst().ScheduleParallel();
        UpdateECB();
    }
}

public struct OrderComponent : IComponentData
{
    public OrderType orderType;
    public int targetTileIndex;
    public Entity targetEntity;
    public int lifetime;
    public const int defaultOrderLifeTime = 100;
    public OrderComponent(OrderType orderType, TileData targetTile, int lifetime = defaultOrderLifeTime) : this()
    {
        this.orderType = orderType;
        this.targetTileIndex = targetTile.index;
        this.lifetime = lifetime;
        this.targetEntity = Entity.Null;
    }

    public OrderComponent(OrderType orderType, Entity targetEntity, int lifetime = defaultOrderLifeTime) : this()
    {
        this.orderType = orderType;
        this.targetEntity = targetEntity;
        this.lifetime = lifetime;
        this.targetTileIndex = -1;
    }

    public OrderComponent(OrderType orderType, TileData targetTile, Entity targetEntity, int lifetime = defaultOrderLifeTime)
    {
        this.orderType = orderType;
        this.targetTileIndex = targetTile.index;
        this.targetEntity = targetEntity;
        this.lifetime = lifetime;
    }

    public static OrderComponent Null
    {
        get
        {
            var order = new OrderComponent();
            order.orderType = OrderType.Null;
            return order;
        }
    }

}