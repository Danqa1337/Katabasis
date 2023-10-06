using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class StructureMap : DeepClonable
{
    public readonly int Width;
    public readonly int Height;

    [SerializeField] public readonly int2[] SpawnPoints = new int2[10]
    {
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
        new int2(-1,-1),
    };

    [SerializeField] public readonly TileTemplate[] Map;
    public StructureMap(int width, int height, int2[] spawnPoints, TileTemplate[] map)
    {
        Width = width;
        Height = height;
        SpawnPoints = spawnPoints;
        Map = map;
    }
    public StructureMap Turn()
    {
        var turnedMap = new TileTemplate[Height * Width];
        var turnedSpawnPoints = new int2[10];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var flatIndex = new int2(x, y).ToMapIndex(Width, Height);
                var turnedIndex = new int2(y, x).ToMapIndex(Height, Width);
                turnedMap[turnedIndex] = Map[flatIndex];
            }
        }

        for (int i = 0; i < SpawnPoints.Length; i++)
        {
            turnedSpawnPoints[i] = new int2(SpawnPoints[i].y, SpawnPoints[i].x);
        }

        return new StructureMap(Height, Width, turnedSpawnPoints, turnedMap);
    }
    public void SetTemplate(TileTemplate tileTemplate, int x, int y)
    {
        var index = new int2(x, y).ToMapIndex(Width, Height);
        Map[index] = tileTemplate;
    }
    public void SetTemplate(TileTemplate tileTemplate, int index)
    {
        Map[index] = tileTemplate;
    }

}






