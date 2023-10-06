using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class SeamlessTextureSystem : MySystemBase
{
    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<SeamlessTextureTag>());
        if (!query.IsEmpty)
        {
            Entities.WithAll<SeamlessTextureTag>().ForEach((Entity entity, in SimpleObjectNameComponent iDComponent, in CurrentTileComponent currentTileComponent, in ObjectTypeComponent objectTypeComponent) =>
            {
                var collection = SpriteCollectionsDatabase.GetSpriteCollection(iDComponent.simpleObjectName);
                var spriteNum = SeamlessTextureAligner.GetSpriteNum(entity);
                var sprite = collection.sprites[spriteNum];
                var position = currentTileComponent.currentTileId.ToMapPosition();
                var objectType = objectTypeComponent.objectType;
                if (entity.HasComponent<RendererComponent>())
                {
                    var renderer = entity.GetComponentObject<RendererComponent>();
                    if (spriteNum > renderer.Sprites.Count - 1)
                    {
                        Debug.Log(spriteNum + " " + renderer.Sprites.Count);
                    }
                    renderer.DrawCollection(collection, spriteNum, renderer.AltSpriteDrown);
                }
                else
                {
                    FloorBaker.DrawTile(position, sprite, objectType);
                }
            }).WithoutBurst().Run();
        }
    }
}