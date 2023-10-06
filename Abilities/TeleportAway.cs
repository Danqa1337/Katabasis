using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class TeleportAway : Ability
    {
        public override AbilityName AbilityName => AbilityName.TeleportAway;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            var freeTiles = self.CurrentTile().GetTilesInRadius(3).Where(t => t.SolidLayer == Entity.Null && !t.isAbyss && t.ClearLineOfSight(self.CurrentTile())).ToList();
            ecb.RemoveComponent<TraderComponent>(self);
            if (freeTiles.Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    var tile = freeTiles.RandomItem();
                    freeTiles.Remove(tile);
                    //var guard = ObjectDataFactory.GetRandomCreature(Registers.GlobalMapRegister.depth, itemName: ComplexObjectName.HumanGuard, withRandomEquip: true);
                    //guard.squadMemberComponent = new ComponentReferece<SquadMemberComponent>(new SquadMemberComponent(self.GetComponentData<SquadMemberComponent>().squadIndex));
                    //SpawnSystem.ScheduleSpawn(guard, tile);
                    TempObjectSystem.SpawnTempObject(TempObjectType.SacrificialFlame, tile);
                }
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return 0;
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return true;
        }
    }
}