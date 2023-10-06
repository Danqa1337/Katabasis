using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

[DisableAutoCreation]
public partial class SpillLiquidSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        Entities.ForEach((Entity entity, in InternalLiquidComponent internalLiquidComponent, in SpillLiquidComponent spillLiquidComponent, in CurrentTileComponent currentTileComponent) =>
        {
            var tiles = new List<TileData>() { currentTileComponent.CurrentTile };
            var currentTile = currentTileComponent.currentTileId.ToTileData();
            if (spillLiquidComponent.radius > 0)
            {
                tiles.AddRange(currentTile.GetNeiborsSafe(true));
            }

            tiles = tiles.Where(t => !t.visionBlocked && !t.isAbyss).ToList();
            if (tiles.Count > 0)
            {
                for (int i = 0; i < spillLiquidComponent.amount; i++)
                {
                    SpawnSystem.ScheduleSpawn(internalLiquidComponent.liquidSpaltter, tiles.RandomItem());
                }
            }

            SoundSystem.ScheduleSound(SoundName.Splash, currentTileComponent.CurrentTile);
            ecb.RemoveComponent<SpillLiquidComponent>(entity);
        }).WithoutBurst().Run();
        UpdateECB();
    }
}

public struct SpillLiquidComponent : IComponentData
{
    public int radius;
    public int amount;

    public SpillLiquidComponent(int radius, int amount)
    {
        this.radius = radius;
        this.amount = amount;
    }
}