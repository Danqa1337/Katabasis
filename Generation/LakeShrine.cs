using System.Linq;

namespace GenerationModules
{
    public class LakeShrine : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            var shrineData = StructuresDatabase.GetStructureData(StructureName.LakeShrine1);
            var tiles = generationData.Regions[0].Tiles.ToList();
            tiles.Shuffle();

            foreach (var tile in tiles)
            {
                if (StructureBuilder.CheckFreeSpaceForStructure(tile, shrineData))
                {
                    StructureBuilder.BuildStructure(tile, shrineData);
                    Spawner.Spawn(ComplexObjectName.Ea, tile);
                    return;
                }
            }
        }
    }
}