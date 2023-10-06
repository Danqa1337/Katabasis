using UnityEngine;

namespace GenerationModules
{
    public class Floor : GenerationModule
    {
        [SerializeField] private SimpleObjectName _floorMaterial;
        public override void Generate(GenerationData generationData)
        {
            foreach (var tile in GetAllMapTiles())
            {
                if(tile.Template.isFloor())
                {
                    tile.Template.GenerateObject(_floorMaterial);
                }    
            }
        }
    }

}

