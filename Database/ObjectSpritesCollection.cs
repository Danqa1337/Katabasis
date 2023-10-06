using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class ObjectSpritesCollection
{
    public SimpleObjectName name;

    [SerializeField]
    public List<Sprite> sprites = new List<Sprite>();

    [SerializeField]
    public List<Sprite> alternativeSprites = new List<Sprite>();

    public float2 spriteCenterOffset = new float2(0.5f, 0.5f);
    public bool hasSeamlessTexture;

    public ObjectSpritesCollection(SimpleObjectName name)
    {
        this.name = name;
        var simpleObjectData = SimpleObjectsDatabase.GetObjectData(name);

        sprites = new List<Sprite>();

        if (!LowLevelSettings.instance.useAtlases)
        {
            var path = "";
            path = "Sprites/Objects/";

            path += name;
            var bodyPartTag = simpleObjectData.bodyPartComponent.IsValid ? simpleObjectData.bodyPartComponent.Component.bodyPartTag.ToString() : "";

            sprites = Resources.LoadAll<Sprite>(path + bodyPartTag).ToList();
            if (sprites.Count == 0)
            {
                sprites = Resources.LoadAll<Sprite>(path).ToList();
            }
            if (sprites.Count == 0)
            {
                Debug.Log(name + " sprite is missing " + path);
            }

            alternativeSprites = Resources.LoadAll<Sprite>(path + bodyPartTag + "Alt").ToList();
            if (alternativeSprites.Count == 0)
            {
                alternativeSprites = Resources.LoadAll<Sprite>(path + "Alt").ToList();
            }
        }
        else //atlas case
        {
        }

        if (sprites.Count == 0)
        {
            //  Debug.Log(path + " has no sprite");
        }
        else
        {
            if (sprites.Count == 64)
            {
                hasSeamlessTexture = true;
            }
            else
            {
                hasSeamlessTexture = false;
                CalculateSpriteCenterOffset();
            }
        }
    }

    public void CalculateSpriteCenterOffset()
    {
        if (sprites.Count > 0 && sprites[0] != null)
        {
            var texture = sprites[0].texture;
            var rect = sprites[0].textureRect;
            int pixelNum = 0;
            var positionSum = int2.zero;

            var upperExtremum = 0;
            var lowerExtremum = 0;
            var rightExtremum = 0;
            var leftExtremum = 0;

            for (int y = (int)rect.yMin; y < rect.yMax; y++)
            {
                for (int x = (int)rect.xMin; x < rect.xMax; x++)
                {
                    if (texture.GetPixel(x, y).a > 0)
                    {
                        var horizontalExtend = x - 15;
                        var vericlaExtend = y - 15;

                        if (vericlaExtend > upperExtremum) upperExtremum = vericlaExtend;
                        if (vericlaExtend < lowerExtremum) lowerExtremum = vericlaExtend;
                        if (horizontalExtend < leftExtremum) leftExtremum = horizontalExtend;
                        if (horizontalExtend > rightExtremum) rightExtremum = horizontalExtend;
                    }
                }
            }

            var center = new float2((leftExtremum + rightExtremum) / 2f, (upperExtremum + lowerExtremum) / 2f) / 32f / 100;

            spriteCenterOffset = -center;
        }
    }
}