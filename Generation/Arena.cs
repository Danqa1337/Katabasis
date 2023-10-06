using Unity.Mathematics;

namespace GenerationModules
{
    public class Arena : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            TileData leftCorner = new int2(0, 0).ToTileData();

            StructureBuilder.BuildStructure(leftCorner, StructuresDatabase.GetStructureData(StructureName.TestArena));
        }
    }
}