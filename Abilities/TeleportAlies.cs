using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class TeleportAlies : Ability
    {
        public override AbilityName AbilityName => AbilityName.TeleportAlies;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            foreach (var squadMate in Registers.SquadsRegister.GetSquadmates(self.GetComponentData<SquadMemberComponent>().squadIndex))
            {
                var tiles = self.CurrentTile().GetTilesInRadius(2).Where(t => t.SolidLayer == Entity.Null && t.visible && t.ClearTraectory(self.CurrentTile())).ToList().Shuffle().ToList();
                if (squadMate != self)
                {
                    if (tiles.Count == 0) break;
                    if (tiles.All(t => !t.isWalkable(squadMate))) continue;

                    var ai = squadMate.GetComponentData<AIComponent>();
                    ai.abilityCooldown += 10;
                    ai.agressionCooldown = 0;
                    ai.targetSearchCooldown = 0;
                    ecb.AddComponent(squadMate, new MoveComponent(squadMate.CurrentTile(), tiles.First(t => t.isWalkable(squadMate)), MovemetType.SelfPropeled));
                    ecb.SetComponent(squadMate, ai);
                }
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return Registers.SquadsRegister.GetSquadmates(self.GetComponentData<SquadMemberComponent>().squadIndex).Count > 1;
        }
    }
}