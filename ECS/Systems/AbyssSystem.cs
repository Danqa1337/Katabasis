using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisableAutoCreation]
public partial class AbyssSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<DropToAbyssTag>());
        if (!query.IsEmpty)
        {
            var entitiesToDrop = query.ToEntityArray(Unity.Collections.Allocator.TempJob);

            foreach (var entity in entitiesToDrop)
            {
                entity.RemoveComponent<DropToAbyssTag>();
                //if(entity.CurrentTile().visible)
                //{
                //    await DrawDropAnimation(entity);
                //}
                if (!entity.IsPlayer())
                {
                    Registers.GlobalMapRegister.Pit.AddIncomingObject(EntitySerializer.SerializeAsComplexObject(Player.PlayerEntity, World.DefaultGameObjectInjectionWorld.EntityManager));

                    entity.CurrentTile().Remove(entity);
                    entity.AddComponentData(new DestroyEntityTag());
                    entity.RemoveComponent<DropToAbyssTag>();
                    Registers.SquadsRegister.RemoveFromAnySquads(entity);

                    Debug.Log(entity.GetName() + " fell into the Pit");
                }
                else
                {
                    Debug.Log("player fell into the Pit");
                    if (Registers.GlobalMapRegister.CurrentLocation.GenerationPreset == GenerationPresetName.Pit) throw new Exception("you are allready in the Pit");

                    //MainCameraHandler.Camera.transform.SetParent(null);
                    // DrawDropAnimation(PlayerAbilitiesSystem.playerEntity);
                    Player.PlayerEntity.RemoveComponent<DropToAbyssTag>();

                    var pit = Registers.GlobalMapRegister.Pit;
                    bool pitIsGenerated = pit.IsGenerated;

                    LocationEnterManager.MoveToLocation(pit, null);

                    //if (pitIsGenerated) LocationGenerator.instance.GetComponent<PitGenerationPreset>().SpawnPlayer(null);
                    return;
                }
            }

            entitiesToDrop.Dispose();
        }
    }

    private static async Task DrawDropAnimation(Entity entity)
    {
        foreach (var item in entity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())

        {
            item.GetComponentObject<RendererComponent>().spritesSortingLayer = SortingLayer.Floor.ToString();
        }

        var transform = entity.GetComponentObject<EntityAuthoring>().transform;
        for (int i = 0; i < 15; i++)
        {
            transform.localScale = new Vector3(transform.localScale.x * 0.9f, transform.localScale.y * 0.9f, transform.localScale.z * 0.9f);
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
            await Task.Delay(LowLevelSettings.instance.majorFrameDrawInterval);
        }
    }
}