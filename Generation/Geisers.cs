using System.Linq;
using System.Collections.Generic;

namespace GenerationModules
{
    public class Geisers : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            List<TileData> tiles = GetAllMapTiles().Where(t => t.Template.templateState == TemplateState.Floor && t.GetDistanceFromEdge() > 5).ToList();

            foreach (var tile in tiles)
            {
                HashSet<TileData> tilesInRadius = BaseMethodsClass.GetTilesInRadius(tile, 3);
                bool radiusIsFree = true;
                foreach (var neibor in tilesInRadius)
                {
                    if (neibor.SolidLayer != null)
                    {
                        radiusIsFree = false;
                        break;
                    }
                }
                if (radiusIsFree)
                {
                    tile.Template.GenerateObject(SimpleObjectName.Geiser);

                    foreach (var neibor in tilesInRadius)
                    {
                        if (!tile.GetNeibors(true).Contains(neibor) && neibor.Template.templateState == TemplateState.Floor && neibor != tile && Chance(80)) neibor.Template.GenerateObject(SimpleObjectName.ShallowWater);
                    }
                    break;
                }
            }
        }
    }
}