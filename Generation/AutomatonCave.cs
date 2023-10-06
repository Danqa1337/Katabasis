using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using System;
using UnityEngine.UIElements;

namespace GenerationModules
{
    public abstract class AutomatonModule : GenerationModule
    {
        [SerializeField][Range(0, 10)] protected float automatonLacunarity = 2.5f;
        [SerializeField][Range(0, 10)] protected float automatonPersistance = 6;
        [SerializeField] protected float automatonBorderFactor = 0.1f;
        [SerializeField] protected int scale = 35;
        [SerializeField] protected float automatonWallPercent = 80;
        [SerializeField] protected int _iterations = 5;

        public override void Generate(GenerationData generationData)
        {
            throw new NotImplementedException();
        }
    }

    public class AutomatonCave : AutomatonModule
    {
        public Texture2D texture2D;

        public override void Generate(GenerationData generationData)
        {
            var noiseMap = NoiseGen.GenerateNoiseMap(64, 64, (int)UnityEngine.Random.Range(-10000, 10000), scale, 3, automatonPersistance, automatonLacunarity, Vector2.zero);

            var wallPercent = automatonWallPercent;
            var smoothedMap = new float[64, 64];
            int[,] tileStates = CelularAutomaton.ProcessMap(wallPercent, _iterations, automatonBorderFactor, noiseMap, out smoothedMap);
            generationData.ElevationMap = smoothedMap;

            for (int x = 0; x < tileStates.GetLength(0); x++)
            {
                for (int y = 0; y < tileStates.GetLength(1); y++)
                {
                    if (Debug) texture2D.SetPixel(x, y, Color.Lerp(Color.black, Color.white, generationData.ElevationMap[x, y]));
                    var tile = new int2(x, y).ToTileData();

                    var elevation = generationData.ElevationMap[x, y];
                    var biome = elevation <= 0.9f ? Biome.Cave : Biome.Wall;
                    TemplateState templateState = elevation <= 0.9f ? TemplateState.Floor : TemplateState.Darkness;

                    tile.Template.SetState(templateState);
                    tile.Template.SetBiome(biome);
                }
            }

            if (Debug) texture2D.Apply();
        }
    }
}