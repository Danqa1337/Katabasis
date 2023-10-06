using Unity.Entities;
using UnityEngine;
[DisableAutoCreation]
public partial class EntityDestructionSystem : MySystemBase
{

    protected override void OnUpdate()
    {
        var ecb = CreateEntityCommandBuffer();
        Entities.WithAll<DestroyEntityTag>().ForEach((Entity entity) =>
        {


            if (entity.HasComponent<EntityAuthoring>())
            {
                entity.GetComponentObject<EntityAuthoring>().Destroy();


            }
            if (entity.HasComponent<CurrentTileComponent>())
            {
                entity.GetComponentData<CurrentTileComponent>().currentTileId.ToTileData().Remove(entity);
            }
            Debug.Log(entity.GetName() + " is destroyed");

            ecb.DestroyEntity(entity);


        }).WithoutBurst().Run();

        UpdateECB();
    }
}


