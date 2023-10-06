using System.Linq;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class CreatureEvasionSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        Entities.WithAll<CreatureComponent>().WithNone<MoveComponent>().WithNone<ImpulseComponent>().WithNone<IsGoingToSwapComponent>().ForEach(
        (Entity creature, ref CreatureComponent creatureComponent, ref PhysicsComponent physics, in CurrentTileComponent currentTileComponent) =>
        {
            if (BaseMethodsClass.Chance(150f / StatsCalculator.CalculateMovementCost(creature, currentTileComponent.CurrentTile) + ((2 - (int)physics.size)) * 10) && !creature.GetTags().Contains(Tag.Dummy))
            {
                bool dangerFound = false;
                Collision dangerCollision = new Collision();

                if (dangerFound)
                {
                    var tiles = creature.CurrentTile().GetNeibors(true).ToList();
                    tiles.Shuffle();
                    foreach (var tile in tiles)
                    {
                        if (tile.isWalkable(creature))
                        {
                            var moveComponent = new MoveComponent
                            {
                                prevTileId = creature.CurrentTile().index,
                                nextTileId = tile.index,
                                movemetType = MovemetType.SelfPropeled
                            };

                            Debug.Log(creature.GetName() + " Evaded!");
                            ecb.AddComponent(creature, moveComponent);
                            break;
                        }
                    }
                }
            }

        }).WithoutBurst().Run();
        UpdateECB();
    }
}
