using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class SpriteTickAnimationSystem : MySystemBase
{
    private int cloudMoveCooldown = 10;

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, RendererComponent rendererComponent, in OnTickAnimationComponent onTickAnimationComponent, in CurrentTileComponent currentTileComponent) =>
        {
            if (currentTileComponent.CurrentTile.visible)
            {
                var collection = rendererComponent.ObjectSpritesCollection;
                Debug.Log(rendererComponent.SpriteIndex + " " + (rendererComponent.SpriteIndex + 1) % collection.sprites.Count);
                rendererComponent.DrawCollection(collection, (rendererComponent.SpriteIndex + 1) % collection.sprites.Count, rendererComponent.AltSpriteDrown);
            }
        }).WithoutBurst().Run();
    }
}

public struct OnTickAnimationComponent : IComponentData
{
}