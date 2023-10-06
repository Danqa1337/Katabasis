using System.Linq;

namespace GenerationModules
{
    public class Doors : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            foreach (var item in generationData.Arcs.Where(t => t.Template.SolidLayer == SimpleObjectName.Null && t.isPlaceForDoor()))
            {
                if (Chance(30)) item.Template.GenerateObject(SimpleObjectName.Door);
            }
        }
    }
}