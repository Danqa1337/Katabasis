using UnityEngine;

public static class NoiseGen
{



    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 000.1f;
        }
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfHaight = mapHeight / 2f;

        float halfWidth = mapWidth / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHaight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;

                }
                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);

            }


        }
        return noiseMap;
    }
    public static float[,] GenerateBlankMap()
    {
        float[,] map = new float[1, 1];
        map[0, 0] = 1f;
        return map;
    }

}

