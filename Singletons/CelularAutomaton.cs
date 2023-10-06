using System.Collections.Generic;
using UnityEngine;

public class CelularAutomaton : MonoBehaviour
{
    private static int[,] oldCells;
    private static int[,] newCells;

    private static List<Vector2Int> neibors;
    public Texture2D texture;

    public AnimationCurve Curve;

    [ExecuteInEditMode]
    [ContextMenu("Generate")]
    public static int[,] GenerateMap(int size, float wallPercent, int iterationsCount, int scale, float pers, float lac, float borderFactor)
    {
        var initialMap = NoiseGen.GenerateNoiseMap(size, size, Random.Range(-10000, 10000), scale, 3, pers, lac, Vector2.zero);
        var result = new float[64, 64];
        return ProcessMap(wallPercent, iterationsCount, borderFactor, initialMap, out result);
    }

    public static int[,] ProcessMap(float wallPercent, int iterationsCount, float borderFactor, float[,] initialMap, out float[,] smothedMapResult)
    {
        var sizeX = initialMap.GetLength(0);
        var sizeY = initialMap.GetLength(1);

        var smothedMap = initialMap;

        oldCells = new int[sizeX, sizeY];
        newCells = new int[sizeX, sizeY];

        neibors = new List<Vector2Int>();

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var distanceFromEdge = Mathf.Min(y, x, sizeX - x, sizeY - y);

                if ((y == 0 || x == 0 || y == sizeY - 1 || x == sizeX - 1)
                    || KatabasisUtillsClass.Chance((Mathf.Clamp01(initialMap[x, y])
                    * wallPercent / Mathf.Clamp(distanceFromEdge
                    / (sizeX * 0.5f), borderFactor, 1f))))
                {
                    oldCells[x, y] = 1;
                }
                else oldCells[x, y] = 0;
            }
        }

        for (int i = 0; i < iterationsCount; i++)
        {
            Iteration();
        }

        smothedMapResult = smothedMap;
        return oldCells;

        void Iteration()
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeX; y++)
                {
                    getNeibors(x, y);

                    int w = 0;
                    int f = 0;

                    foreach (var item in neibors)
                    {
                        if (GetState(item) == 1) w++;
                        if (GetState(item) == 0) f++;
                    }

                    if (neibors.Count == 8)
                    {
                        smothedMap[x, y] = (smothedMap[x, y] + w / 8f) / 2f;
                    }
                    else
                    {
                        smothedMap[x, y] = 1;
                    }

                    if (w > 4 || neibors.Count < 8)
                    {
                        newCells[x, y] = 1;
                    }
                    if (f > 4)
                    {
                        newCells[x, y] = 0;
                    }
                }
            }
            oldCells = newCells;
        }

        void getNeibors(int X, int Y)
        {
            neibors.Clear();

            for (int x = X - 1; x <= X + 1; x++)
            {
                for (int y = Y - 1; y <= Y + 1; y++)
                {
                    if (x >= 0 && y >= 0 && x < sizeY && y < sizeX) neibors.Add(new Vector2Int(x, y));
                }
            }
            neibors.Remove(new Vector2Int(X, Y));
        }
    }

    [ContextMenu("Iteration")]
    private static int GetState(int x, int y)
    {
        return oldCells[x, y];
    }

    private static int GetState(Vector2Int vector)
    {
        return oldCells[vector.x, vector.y];
    }
}