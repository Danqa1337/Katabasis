using System.Collections.Generic;

namespace GenerationModules
{
    public class SpawnIncomingObjects : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            foreach (var item in generationData.Location.IncomeingComplexObjects)
            {
                Spawner.Spawn(item, GetFreeTile());
            }
            foreach (var item in generationData.Location.IncomeingSimpleObjects)
            {
                Spawner.Spawn(item, GetFreeTile());
            }

            generationData.Location.ClearIncomingObjects();
        }
    }
}