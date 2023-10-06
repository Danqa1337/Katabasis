using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Dig : Ability
    {
        public override AbilityName AbilityName => AbilityName.Dig;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var selfCurrentTile = self.CurrentTile();

            ecb.AddComponent(abilityComponent.targetTile.SolidLayer, new BreakObjectComponent(self, DurabilityChangeReason.Smashed));

            abilityComponent.targetTile.SolidLayer = Entity.Null;
            abilityComponent.targetTile.Save();
            SpawnSystem.ScheduleSpawn(SimpleObjectName.RockDebris, selfCurrentTile);
            TempObjectSystem.SpawnTempObject(TempObjectType.LargeDust, selfCurrentTile);
            TempObjectSystem.SpawnTempObject(TempObjectType.LargeDust, abilityComponent.targetTile);

            var MakeStep = new MakeStep();
            if (MakeStep.TileFunc(self, abilityComponent)) MakeStep.DoAction(self, abilityComponent, ecb);
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile.SolidLayer != Entity.Null && abilityComponent.targetTile.SolidLayer.HasTag(Tag.Wall);
        }
    }
}