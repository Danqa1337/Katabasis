using Unity.Entities;

public struct AbilityComponent : IComponentData
{
    public AbilityName ability;
    public TileData targetTile;
    public Entity targetEntity;
    public int power;

    public AbilityComponent(AbilityName ability, TileData targetTile, int Power = 0)
    {
        this.ability = ability;
        this.targetTile = targetTile;
        this.targetEntity = Entity.Null;
        this.power = Power;
    }

    public AbilityComponent(AbilityName ability, int Power = 0)
    {
        this.ability = ability;
        this.targetTile = TileData.Null;
        this.targetEntity = Entity.Null;
        this.power = Power;
    }

    public AbilityComponent(AbilityName ability, Entity targetEntity, int Power = 0)
    {
        this.ability = ability;
        this.targetTile = TileData.Null;
        this.targetEntity = targetEntity;
        this.power = Power;
    }

    public AbilityComponent(AbilityName ability, TileData targetTile, Entity targetEntity, int Power = 0)
    {
        this.ability = ability;
        this.targetTile = targetTile;
        this.targetEntity = targetEntity;
        this.power = Power;
    }
}