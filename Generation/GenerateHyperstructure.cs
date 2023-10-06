using Unity.Mathematics;

namespace GenerationModules
{
    public class GenerateHyperstructure : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            if (generationData.Location.HyperstuctureName != HyperstuctureName.Null)
            {
                StructuresDatabase.GetHyperstructure(generationData.Location.HyperstuctureName).Generate(new int2(32, 32).ToTileData());
            }
        }
    }
}