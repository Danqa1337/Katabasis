using System;
using Unity.Entities;

namespace Abilities
{
    [Serializable]
    public abstract class Ability
    {
        public abstract AbilityName AbilityName { get; }

        public abstract void DoAction(Entity self, AbilityComponent abilityComponent, EntityCommandBuffer ecb);

        public abstract bool TileFunc(Entity self, AbilityComponent abilityComponent);

        public abstract int AdditionalCooldown(Entity self, AbilityComponent abilityComponent);

        public static bool debug => World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<AbilitySystem>().debug;

        public static void AddToLastDebugMessage(string message)
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<AbilitySystem>().AddToLastDebugMessage(message);
        }
    }
}