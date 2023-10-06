using Assets.Scripts;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;

public enum DurabilityChangeReason
{
    Smashed,
    Eaten,
    Burned,
    Healed,
    InstantDamage,
}

[DisableAutoCreation]
public partial class ObjectBreakSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();

        Entities.ForEach((Entity entity, in BreakObjectComponent breakObjectComponent, in CurrentTileComponent currentTileComponent, in ObjectSoundData soundData) =>
        {
            if (entity.IsPlayer())
            {
                Player.Die(breakObjectComponent.ResponsibleEntity);
            }
            else
            {
                if (entity.HasComponent<AliveTag>())
                {
                    ecb.AddComponent(entity, new KillCreatureComponent(breakObjectComponent.ResponsibleEntity));

                    var parts = entity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull().Where(p => p != entity).ToList();
                    if (parts.Count > 0)
                    {
                        ecb.AddBuffer<AnatomyChangeElement>(entity);
                        foreach (var item in parts)
                        {
                            var bodyPartTag = item.GetComponentData<BodyPartComponent>().bodyPartTag;
                            ecb.AppendToBuffer(entity, new AnatomyChangeElement(Entity.Null, bodyPartTag, breakObjectComponent.ResponsibleEntity));
                        }
                    }
                }

                if (entity.HasComponent<InventoryComponent>())
                {
                    if (!entity.HasBuffer<ChangeInventoryElement>())
                    {
                        ecb.AddBuffer<ChangeInventoryElement>(entity);
                    }
                    foreach (var item in entity.GetComponentData<InventoryComponent>().items)
                    {
                        ecb.AppendToBuffer(entity, new ChangeInventoryElement(item, false));
                    }
                }

                currentTileComponent.currentTileId.ToTileData().Remove(entity);

                if (entity.HasComponent<LOSBlockTag>())
                {
                    FOVSystem.ScheduleFOWUpdate();
                }

                if (entity.HasComponent<ExplosiveTag>())
                {
                    ExplosionSystem.ScheduleExplosion(currentTileComponent.currentTileId.ToTileData(), 100, breakObjectComponent.ResponsibleEntity);
                }

                if (entity.HasComponent<Parent>())
                {
                    var parent = entity.GetComponentData<Parent>().Value;
                    if (entity.HasComponent<BodyPartComponent>())
                    {
                        ecb.AddBufferElement(parent, new AnatomyChangeElement(Entity.Null, entity.GetComponentData<BodyPartComponent>().bodyPartTag, breakObjectComponent.ResponsibleEntity));
                    }
                    else
                    {
                        ecb.AddBufferElement(parent, new ChangeEquipmentElement(Entity.Null, parent.GetComponentData<EquipmentComponent>().GetEquipTag(entity)));
                    }
                }

                if (entity.HasBuffer<DropElement>())
                {
                    foreach (var item in entity.GetBuffer<DropElement>())
                    {
                        if (BaseMethodsClass.Chance(item.chance)) SpawnSystem.ScheduleSpawn(item.itemName, entity.CurrentTile());
                    }
                }

                if (breakObjectComponent.DamageType == DurabilityChangeReason.Smashed && entity.HasComponent<InternalLiquidComponent>())
                {
                    ecb.AddComponent(entity, new SpillLiquidComponent(1, 3));
                }

                if (entity.HasComponent<SquadMemberComponent>())
                {
                    Registers.SquadsRegister.RemoveFromAnySquads(entity);
                    ecb.RemoveComponent<SquadMemberComponent>(entity);
                }
                if (currentTileComponent.CurrentTile.isAltar)
                {
                    ecb.AddComponent(entity, new SacrificeData(entity, breakObjectComponent.DamageType));
                }
                ecb.AddComponent(entity, new DrawDestroyAnimation());
                ecb.AddComponent(entity, new DestroyEntityTag());
                SoundSystem.ScheduleSound(soundData.BreakSound, currentTileComponent.CurrentTile);
            }
            ecb.RemoveComponent<BreakObjectComponent>(entity);
        }).WithoutBurst().Run();

        UpdateECB();
        ecb = CreateEntityCommandBuffer();
        Entities.WithAll<DrawDestroyAnimation>().ForEach((Entity entity, RendererComponent rendererComponent) =>
        {
            rendererComponent.Desolve();
            ecb.RemoveComponent<DrawDestroyAnimation>(entity);
        }
        ).WithoutBurst().Run();
        UpdateECB();
    }

    private struct DrawDestroyAnimation : IComponentData
    {
    }
}