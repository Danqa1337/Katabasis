using System.Collections;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Surgeon
    {
        public static void Resurrect(Entity entity)
        {
            if (entity.HasComponent<CreatureComponent>() && !entity.HasComponent<AliveTag>())
            {
                var currentTile = entity.CurrentTile();
                entity.AddComponentData(new AliveTag());
                entity.AddComponentData(new AIComponent(entity, 0, currentTile.position));
                entity.SetComponentData(new ObjectTypeComponent(ObjectType.Solid));
                entity.AddComponentData(new MoveComponent(currentTile, currentTile, MovemetType.SelfPropeled));
                entity.AddComponentData(new MoraleComponent());
                entity.GetComponentObject<RendererComponent>().transform.rotation = Quaternion.Euler(Vector3.zero);
                var squadIndex = Registers.SquadsRegister.RegisterNewSquad();
                entity.AddComponentData(new SquadMemberComponent());
                Registers.SquadsRegister.MoveToSquad(squadIndex, entity);
                entity.AddBuffer<MoraleChangeElement>();
            }
        }

        public static void RestorePart(Entity creature, BodyPartTag bodyPartTag)
        {
            var creatureData = ComplexObjectsDatabase.GetObjectData(creature.GetComponentData<ComplexObjectNameComponent>().complexObjectName, true);
            var partData = creatureData.BodyParts.First(b => b.bodyPartComponent.IsValid && b.bodyPartComponent.Component.bodyPartTag == bodyPartTag);
            var newPart = Spawner.Spawn(partData, creature.CurrentTile());
            SewPart(creature, newPart);
        }

        public static void SewPart(Entity creature, Entity newPart)
        {
            var bodyPartTag = newPart.GetComponentData<BodyPartComponent>().bodyPartTag;
            var existingPart = creature.GetComponentData<AnatomyComponent>().GetBodyPart(bodyPartTag);
            if (existingPart != Entity.Null)
            {
                creature.SetZeroSizedTagComponentData(new PerformingSurgeryTag());
                Amputate(creature, existingPart, creature);
            }

            newPart.CurrentTile().Remove(newPart);
            creature.AddBufferElement(new AnatomyChangeElement(newPart, bodyPartTag, creature));

            SoundSystem.ScheduleSound(SoundName.RestorePart, creature.CurrentTile());
        }

        public static void Amputate(Entity creature, Entity part, Entity responsibleEntity)
        {
            creature.AddBufferElement(new AnatomyChangeElement(Entity.Null, part.GetComponentData<BodyPartComponent>().bodyPartTag, responsibleEntity));
        }
    }

    public struct AliveTag : IComponentData
    {
    }
}