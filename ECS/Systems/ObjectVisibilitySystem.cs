using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
[DisableAutoCreation]
public partial class ObjectVisibilitySystem : MySystemBase
{
    protected override void OnUpdate()
    { 
        var ecb = CreateEntityCommandBuffer();
        Entities.WithNone<MapableTag, Parent>().WithAll<RendererComponent>().ForEach((Entity entity) =>
        {
            ecb.AddComponent(entity, new AdjustVisibility());

        }).WithoutBurst().Run();

        UpdateECB();
        ecb = CreateEntityCommandBuffer();

        Entities.WithNone<Parent>().WithAll<AdjustVisibility>().ForEach((Entity entity, Transform transform, in CurrentTileComponent currentTileComponent) =>
        {
            ecb.RemoveComponent<AdjustVisibility>(entity);
            var visible = currentTileComponent.CurrentTile.visible;
            foreach (var item in transform.GetComponentsInChildren<SpriteRenderer>())
            {
                item.enabled = visible;
            }

        }).WithoutBurst().Run();
        UpdateECB();
    }

    public void AjustVisibility()
    {

    }
}
public struct AdjustVisibility : IComponentData
{

}