using Unity.Mathematics;
using UnityEngine;

namespace GenerationModules
{
    public class AutomatonPoodles : AutomatonModule
    {
        [SerializeField] SimpleObjectName _poodleMaterial;
        public override void Generate(GenerationData generationData)
        {
            var noiseMap = NoiseGen.GenerateNoiseMap(64, 64, (int)UnityEngine.Random.Range(-10000, 10000), scale, 3, automatonPersistance, automatonLacunarity, Vector2.zero);

            
            var wallPercent = automatonWallPercent;
            var smoothedMap = new float[64, 64];
            int[,] tileStates = CelularAutomaton.ProcessMap(wallPercent, _iterations, automatonBorderFactor, noiseMap, out smoothedMap);


            for (int x = 0; x < tileStates.GetLength(0); x++)
            {
                for (int y = 0; y < tileStates.GetLength(1); y++)
                {
                    var tile = new int2(x, y).ToTileData();

                    var elevation = smoothedMap[x, y];
                    if(elevation < 0.5f && tile.Template.SolidLayer == SimpleObjectName.Null)
                    {
                        tile.Template.GenerateObject(_poodleMaterial);
                    }
                        

                }
            }

            
        }
    }

}

