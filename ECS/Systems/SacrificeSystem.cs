using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class SacrificeSystem : MySystemBase
{
    public static event Action<Entity> OnScarifice;

    protected override void OnUpdate()
    {
        var sacrificeQuery = GetEntityQuery(ComponentType.ReadOnly<SacrificeData>());
        if (!sacrificeQuery.IsEmpty)
        {
            var array = sacrificeQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
            foreach (var entity in array)
            {
                entity.SetZeroSizedTagComponentData(new DestroyEntityTag());
                TempObjectSystem.SpawnTempObject(TempObjectType.SacrificialFlame, entity.CurrentTile());
                Registers.GodsRegister.OnSacrifice(entity.GetComponentData<SacrificeData>());
                entity.RemoveComponent<SacrificeData>();
            }
            array.Dispose();
        }
    }
}

public struct SacrificeData : IComponentData
{
    public readonly Entity Entity;
    public readonly DurabilityChangeReason DamageType;

    public SacrificeData(Entity entity, DurabilityChangeReason damageType)
    {
        Entity = entity;
        DamageType = damageType;
    }

    public override string ToString()
    {
        return "Sacrifice of " + Entity.GetName() + " " + DamageType;
    }
}