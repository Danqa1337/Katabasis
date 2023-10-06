using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class Eat : Ability
    {
        public override AbilityName AbilityName => AbilityName.Eat;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var eatableItem = abilityComponent.targetEntity;
            var currentTile = self.GetComponentData<CurrentTileComponent>().currentTileId.ToTileData();
            var nutrition = eatableItem.GetComponentData<EatableComponent>().nutrition;
            var selfCurrentTile = self.CurrentTile();

            if (eatableItem.HasBuffer<EffectOnHitElement>())
            {
                var effectsOnCons = eatableItem.GetBuffer<EffectOnHitElement>();
                if (true)
                {
                    foreach (var effect in effectsOnCons)
                    {
                        ecb.AddBufferElement(self, new EffectElement(effect.EffectName, 100, self));
                    }
                }
            }

            ecb.AddComponent(eatableItem, new BreakObjectComponent(self, DurabilityChangeReason.Eaten));

            foreach (var item in self.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
            {
                ecb.AddBufferElement(item, new DurabilityChangeElement(nutrition, self, DurabilityChangeReason.Healed));
            }

            if (eatableItem.HasComponent<InternalLiquidComponent>())
            {
                ecb.AddComponent(eatableItem, new SpillLiquidComponent(1, 3));
            }

            SoundSystem.ScheduleSound(SoundName.Eat, selfCurrentTile);
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 20;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return true;
        }
    }
}