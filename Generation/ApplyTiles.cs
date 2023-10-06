using UnityEngine;
using System.Linq;

namespace GenerationModules
{
    public class ApplyTiles : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            GenerateAbyssSides(generationData);

            FloorBaker.Clear();

            for (int i = 0; i < 4096; i++)
            {
                LocationMap.MapRefference.Value.blobArray[i].ApplyTemplate();
            }

            if (!Application.isPlaying)
            {
                FloorBaker.Apply();
            }
        }

        private void GenerateAbyssSides(GenerationData generationData)
        {
            TimeTester.StartTest();
            foreach (var item in GetAllMapTiles().Where(t => t.Template.templateState == TemplateState.Abyss))
            {
                item.Template.MakeAbyss();
            }
            //var tiles = GetAllMapTiles().Where(t => t.valid && t.template.FloorLayer == SimpleObjectName.Null && (t + Direction.U).template.FloorLayer != SimpleObjectName.Null);
            //foreach (var tile in tiles)
            //{
            //    tile.template.GenerateObject(SimpleObjectName.AbyssSide);
            //}
            TimeTester.EndTest("Making abyss sides took");
        }
    }
}