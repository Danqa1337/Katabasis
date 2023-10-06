using Assets.Scripts;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public partial class DurabilitySystem : MySystemBase
{
    public static event Action OnPlayersPartDurabilityChanged;

    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBufferParallel();
        var random = GetRandom();
        var triggerPlayerDurabilityChange = false;

        Entities.ForEach((int entityInQueryIndex, Entity entity, ref DurabilityComponent durabilityComponent, in DynamicBuffer<DurabilityChangeElement> durabilityChangeBuffer, in CurrentTileComponent currentTileComponent) =>
        {
            if (durabilityComponent.currentDurability != -1)
            {
                var durabilityChange = 0;
                bool IsCreaturesBodyPart = false;
                var creature = Entity.Null;

                var root = entity.HasComponent<Parent>() ? entity.GetComponentData<Parent>().Value : entity;

                if (root.HasComponent<AliveTag>())
                {
                    IsCreaturesBodyPart = true;
                    creature = root;
                }
                if (root.IsPlayer())
                {
                    triggerPlayerDurabilityChange = true;
                }
                foreach (var durabilityChangeElement in durabilityChangeBuffer)
                {
                    if (durabilityChangeElement.Value != 0)
                    {
                        durabilityChange += durabilityChangeElement.Value;

                        if (entity.HasComponent<AliveTag>() && durabilityComponent.currentDurability + durabilityChange < 0)
                        {
                            ecb.AddComponent(entityInQueryIndex, entity, new KillCreatureComponent());
                            durabilityComponent.currentDurability = random.NextInt(1, durabilityComponent.currentDurability);
                        }
                        else
                        {
                            durabilityComponent.currentDurability = Mathf.Clamp(durabilityComponent.currentDurability + durabilityChangeElement.Value, 0, durabilityComponent.MaxDurability);

                            if (durabilityComponent.currentDurability == 0) //destroy
                            {
                                ecb.AddComponent(entityInQueryIndex, entity, new BreakObjectComponent(durabilityChangeElement.ResponsibleEntity, durabilityChangeElement.DurabilityChangeReason));
                            }

                            if (IsCreaturesBodyPart)
                            {
                                ecb.AddOrAppendBufferElement(entityInQueryIndex, creature, new HealthChangedElement(durabilityChangeElement.Value, durabilityChangeElement.ResponsibleEntity));
                            }
                        }
                    }
                }

                if (LowLevelSettings.instance.showDamageOnObjects && durabilityChange != 0 && currentTileComponent.currentTileId.ToTileData().visible)
                {
                    PopUpCreator.CreatePopUp(entity.CurrentTile().position, durabilityChange.ToString(), Color.grey, 1.6f);
                }
            }

            ecb.RemoveComponent<DurabilityChangeElement>(entityInQueryIndex, entity);
        }).WithoutBurst().ScheduleParallel();
        if (triggerPlayerDurabilityChange)
        {
            ScheduleTriggerEvent(OnPlayersPartDurabilityChanged);
        }
        UpdateECB();
        TriggerEvents();
    }
}

public struct DurabilityChangeElement : IBufferElementData
{
    public readonly int Value;
    public readonly Entity ResponsibleEntity;
    public readonly DurabilityChangeReason DurabilityChangeReason;

    public DurabilityChangeElement(int value, Entity responsibleEntity, DurabilityChangeReason durabilityChangeReason)
    {
        Value = value;
        ResponsibleEntity = responsibleEntity;
        DurabilityChangeReason = durabilityChangeReason;
    }
}

[System.Serializable]
public struct DurabilityComponent : IComponentData
{
    public int currentDurability;
    public int MaxDurability;
    public float GetDurabilityPercent => ((float)currentDurability / (float)MaxDurability) * 100f;

    public DurabilityComponent(SimpleObjectsTable.Param param)
    {
        currentDurability = param.maxDurability;
        MaxDurability = param.maxDurability;
    }

    public DurabilityComponent(int durability)
    {
        currentDurability = durability;
        MaxDurability = durability;
    }
}