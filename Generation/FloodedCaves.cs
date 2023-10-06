using UnityEngine;

namespace GenerationModules
{
    public class FloodedCaves : GenerationModule
    {
        [SerializeField] private float _chance;
        [SerializeField] private float _minRegionSizeToFlood;
        [SerializeField] private float _maxElevationShallow;
        [SerializeField] private float _maxElevationDeep;
        [SerializeField] private SimpleObjectName _bottomMaterial;

        public override void Generate(GenerationData generationData)
        {
            foreach (var region in generationData.Regions)
            {
                if (Chance(_chance) && region.Size > _minRegionSizeToFlood)
                {
                    foreach (var item in region.Tiles)
                    {
                        item.Template.GenerateObject(_bottomMaterial);
                        if (item.Template.templateState == TemplateState.Floor
                            && !item.CheckStateInNeibors(TemplateState.Darkness, true)
                            && generationData.ElevationMap[item.position.x, item.position.y] <= _maxElevationShallow)
                        {
                            item.Template.GenerateObject(SimpleObjectName.ShallowWater);
                        }
                        item.Template.SetBiome(Biome.FloodedCave);
                    }
                    foreach (var item in region.Tiles)
                    {
                        if (!item.CheckStateInNeibors(TemplateState.Floor, true) && generationData.ElevationMap[item.position.x, item.position.y] <= _maxElevationDeep)
                        {
                            item.Template.SetState(TemplateState.Abyss);
                        }
                    }
                }
            }
        }
    }
}