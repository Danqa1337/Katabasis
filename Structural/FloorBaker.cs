using Unity.Mathematics;
using UnityEngine;

public class FloorBaker : Singleton<FloorBaker>
{
    public Texture2D floorTexture;
    public Texture2D liquidTexture;
    private static Color32[] _clearColors;

    public static Color32[] ClearColors
    {
        get
        {
            if (_clearColors == null)
            {
                _clearColors = new Color32[instance.floorTexture.width * instance.floorTexture.height];
                for (int i = 0; i < _clearColors.Length; i++)
                {
                    _clearColors[i] = new Color32(0, 0, 0, 0);
                }
            }

            return _clearColors;
        }
    }

    private void OnEnable()
    {
        LocationEnterManager.OnMovingToLocationCompleted += OnEnterLocation;
        LocationEnterManager.OnExitPrevLocationCompleted += OnExitLocation;
    }

    private void OnDisable()
    {
        LocationEnterManager.OnMovingToLocationCompleted -= OnEnterLocation;
        LocationEnterManager.OnExitPrevLocationCompleted -= OnExitLocation;
    }

    private void OnEnterLocation(Location location)
    {
        Apply();
    }

    private void OnExitLocation(Location location)
    {
        Clear();
    }

    public static void DrawTile(int2 position, Sprite sprite, ObjectType objectType)
    {
        var spriteTexture = sprite.texture;
        var rect = sprite.textureRect;
        var texture = instance.floorTexture;
        switch (objectType)
        {
            case ObjectType.Solid:
                break;

            case ObjectType.Drop:
                break;

            case ObjectType.Floor:
                texture = instance.floorTexture;
                break;

            case ObjectType.Liquid:
                texture = instance.liquidTexture;
                break;

            case ObjectType.GroundCover:
                break;

            case ObjectType.Hovering:
                break;

            default:
                break;
        }

        texture.SetPixels(position.x * 32, position.y * 32, (int)rect.width, (int)rect.height, spriteTexture.GetPixels((int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height), 0);
    }

    public static void Apply()
    {
        instance.floorTexture.Apply();
        instance.liquidTexture.Apply();
    }

    public static void ClearTile(int2 position, ObjectType objectType)
    {
        var colors = new Color[32 * 32];
        var texture = instance.floorTexture;
        switch (objectType)
        {
            case ObjectType.Solid:
                break;

            case ObjectType.Drop:
                break;

            case ObjectType.Floor:
                texture = instance.floorTexture;
                break;

            case ObjectType.Liquid:
                texture = instance.liquidTexture;
                break;

            case ObjectType.GroundCover:
                break;

            case ObjectType.Hovering:
                break;

            default:
                break;
        }
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(position.x * 32, position.y * 32, 32, 32, colors, 0);
    }

    public static void Clear()
    {
        instance.floorTexture.SetPixels32(0, 0, instance.floorTexture.width, instance.floorTexture.height, ClearColors);
        instance.liquidTexture.SetPixels32(0, 0, instance.floorTexture.width, instance.floorTexture.height, ClearColors);
    }
}