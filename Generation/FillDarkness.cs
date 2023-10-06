using System.Linq;
using UnityEngine;
namespace GenerationModules
{
    public class FillDarkness : GenerationModule
    {
        [SerializeField] private SimpleObjectName _rockName = SimpleObjectName.RockWall;
        public override void Generate(GenerationData generationData)
        {
            var maptiles = GetAllMapTiles().Where(t => t.Template.templateState == TemplateState.Darkness);

            foreach (var item in GetAllMapTiles().Where(t => t.Template.templateState == TemplateState.Darkness))
            {

                item.Template.GenerateObject(_rockName);
                
                item.Template.SetBiome(Biome.Wall);
            }
        }
    }

}

