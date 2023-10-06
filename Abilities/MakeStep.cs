using System;
using System.Linq;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public class MakeStep : Ability
    {
        public override AbilityName AbilityName => AbilityName.MakeStep;

        public override void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb)
        {
            if (abilityComponent.targetTile.hasActivePressurePlate && (BaseMethodsClass.Chance(100) && self.IsPlayersSquadmate()))
            {
                var tileToJump = TileData.Null;
                var opositeTile = self.CurrentTile() + (abilityComponent.targetTile - self.CurrentTile()) * 2;
                var goodTilePredicate = new Func<TileData, bool>(t => t != TileData.Null && t != self.CurrentTile() && t.isWalkable(self) && t.SolidLayer == Entity.Null && !t.hasActivePressurePlate);

                if (goodTilePredicate.Invoke(opositeTile))
                {
                    tileToJump = opositeTile;
                }
                else
                {
                    var neibors = abilityComponent.targetTile.GetNeibors(true);
                    var goodNeibors = neibors.Where(goodTilePredicate).ToList();
                    if (goodNeibors.Count > 0)
                    {
                        tileToJump = goodNeibors.RandomItem();
                    }
                }

                if (tileToJump != TileData.Null)
                {
                    abilityComponent.targetTile = tileToJump;
                    if (debug) AddToLastDebugMessage(self.GetName() + " trying to jump over pressure plate");
                    new Jump().DoAction(self, abilityComponent, ecb);
                    return;
                }
            }

            var solid = abilityComponent.targetTile.SolidLayer;
            if (solid.CanBeSwapedRightNow(self))
            {
                //swap creatures

                ecb.AddComponent(self, new IsGoingToSwapComponent(solid));
            }
            else
            {
                if (solid != Entity.Null && solid.HasTag(Tag.Wall) && self.HasComponent<DiggerTag>())
                {
                    new Dig().DoAction(self, abilityComponent, ecb);
                }
                else
                {
                    ecb.AddComponent(self, new MoveComponent(self.CurrentTile(), abilityComponent.targetTile, MovemetType.SelfPropeled));
                }
            }
        }

        public override int AdditionalCooldown(Entity self, AbilityComponent abilityComponent)
        {
            return StatsCalculator.CalculateMovementCost(self, self.CurrentTile());
        }

        public override bool TileFunc(Entity self, AbilityComponent abilityComponent)
        {
            return abilityComponent.targetTile != self.CurrentTile() && abilityComponent.targetTile.isWalkable(self);
        }
    }
}