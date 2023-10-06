using Unity.Entities;
using UnityEngine;

public class PhotoCamera : Singleton<PhotoCamera>
{
    private Camera cam;
    public SpriteRenderer photoStand;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public static void MakePartPhoto(Entity thing, RenderTexture texture)
    {
        var objectTransform = thing.GetComponentObject<RendererComponent>().transform;
        var objectPosition = objectTransform.position;
        var partHolder = thing.GetComponentObject<EntityAuthoring>().partsHolder;
        var root = thing.GetComponentObject<EntityAuthoring>().gameObject.transform.root;

        objectTransform.position = new Vector3(objectPosition.x, objectPosition.y, 1000);
        partHolder.gameObject.SetActive(false);

        MakePhoto(new Vector3(root.transform.position.x, root.transform.position.y, objectTransform.position.z), texture);

        objectTransform.position = objectPosition;
        partHolder.gameObject.SetActive(true);
    }

    public static void MakeFullPhoto(Entity entity, RenderTexture texture)
    {
        instance.cam.targetTexture = texture;
        if (entity.HasComponent<Transform>())
        {
            var objectTransform = entity.GetComponentObject<Transform>();
            var objectPosition = objectTransform.position;

            objectTransform.position = new Vector3(objectPosition.x, objectPosition.y, 1000);

            MakePhoto(objectTransform.position, texture);

            objectTransform.position = objectPosition;
        }
        else
        {
            var rendererData = entity.GetComponentObject<RendererComponent>();
            var spriteCollection = rendererData.ObjectSpritesCollection;
            if (rendererData.AltSpriteDrown)
            {
                instance.photoStand.sprite = spriteCollection.alternativeSprites[rendererData.SpriteIndex];
            }
            else
            {
                instance.photoStand.sprite = spriteCollection.sprites[rendererData.SpriteIndex];
            }
            MakePhoto(instance.photoStand.transform.position, texture);
        }
    }

    private static void MakePhoto(Vector3 position, RenderTexture texture)
    {
        instance.cam.targetTexture = texture;
        instance.cam.transform.position = position;
        instance.cam.enabled = true;
        instance.cam.Render();
        instance.cam.enabled = false;
    }
}