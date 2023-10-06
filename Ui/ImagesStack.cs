using Unity.Entities;
using UnityEngine;

public class ImagesStack : RenderersStack
{
    public void DrawItem(Entity entity)
    {
        Clear();
        if (entity != Entity.Null)
        {
            var renderer = entity.GetComponentObject<RendererComponent>();
            var firstRendererOffset = renderer.ObjectSpritesCollection.spriteCenterOffset;
            renderers[0].transform.localPosition = (firstRendererOffset).ToVector3();
            renderers[0].sprite = renderer.Sprite;
            renderers[0].color = Color.white;
        }
        else
        {
            throw new System.NullReferenceException();
        }
    }
}