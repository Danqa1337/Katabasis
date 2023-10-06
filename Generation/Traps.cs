using UnityEngine;

namespace GenerationModules
{
    public class Traps : GenerationModule
    {
        [SerializeField] private int _trapNum;
        public override void Generate(GenerationData generationData)
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < _trapNum; i++)
                {
                    TileData tile = GetFreeTile();
                    Spawner.Spawn(SimpleObjectName.PressurePlate, tile);
                }
            }
        }
    }

}

