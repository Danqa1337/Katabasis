using Unity.Entities;

public struct WalkabilityDataComponent : IComponentData
{
    public bool isPlayer;
    public bool isPlayersSquadmate;
    public bool hovering;
    public bool flying;
    public bool digger;


    public WalkabilityDataComponent(Entity entity)
    {
        this.isPlayer = entity.IsPlayer();
        this.isPlayersSquadmate = entity.IsPlayersSquadmate();
        this.hovering = entity.GetObjectType() == ObjectType.Hovering;
        this.flying = entity.HasComponent<FlyingTag>();
        this.digger = entity.HasComponent<DiggerTag>();
    }
}
