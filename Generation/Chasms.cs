using UnityEngine;

namespace GenerationModules
{
    public class Chasms : GenerationModule
    {
        [SerializeField] private float _chance;
        [SerializeField] private float _maxElevation;
        [SerializeField] private int _minCahsmSize;

        public override void Generate(GenerationData generationData)
        {
            foreach (var region in generationData.Regions)
            {
                if (region.Size > _minCahsmSize && Chance(_chance))
                {
                    foreach (var tile in region.Tiles)
                    {
                        tile.Template.SetBiome(Biome.Chasm);
                        var elevation = generationData.ElevationMap[tile.x, tile.y];
                        if (elevation < _maxElevation)
                        {
                            if (!tile.CheckStateInNeibors(TemplateState.Darkness, true))
                            {
                                tile.Template.ClearLayer(ObjectType.Floor);
                                tile.Template.ClearLayer(ObjectType.GroundCover);
                                tile.Template.ClearLayer(ObjectType.Solid);
                                tile.Template.SetState(TemplateState.Abyss);
                            }
                        }
                    }
                }
            }
        }
    }
}