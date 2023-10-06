using Unity.Mathematics;
using UnityEngine;

namespace GenerationModules
{
    public class Stairs : GenerationModule
    {
        public override void Generate(GenerationData generationData)
        {
            if (generationData.DebugStairs)
            {
                generationData.StaircaseDebugTile = GetTile(t => t.Template.SolidLayer == SimpleObjectName.Null && t.Template.FloorLayer != SimpleObjectName.Null);
            }

            for (int i = 0; i < generationData.Location.TransitionsIDs.Count; i++)
            {
                var id = generationData.Location.TransitionsIDs[i];

                var transition = Registers.GlobalMapRegister.GetTransition(id);

                var stairsType = SimpleObjectName.StairsDown;
                var tile = GetTile(t => t.Template.SolidLayer == SimpleObjectName.Null && t.Template.FloorLayer != SimpleObjectName.Null);

                if (transition.entranceLocationId == generationData.Location.Id)
                {
                    if (generationData.DebugStairs)
                    {
                        tile = generationData.StaircaseDebugTile + new int2(1, i);
                    }
                    transition.entrancePosition = tile.position;

                    stairsType = SimpleObjectName.StairsDown;
                }
                else
                {
                    if (generationData.DebugStairs)
                    {
                        tile = generationData.StaircaseDebugTile + new int2(-1, i);
                    }
                    transition.exitPosition = tile.position;
                    stairsType = SimpleObjectName.StairsUp;
                }

                var template = tile.Template;
                template.SolidLayer = SimpleObjectName.Null;
                template.FloorLayer = SimpleObjectName.Null;
                template.LiquidLayer = SimpleObjectName.Null;
                template.GroundCover = stairsType;
                tile.Template = template;
                tile.transitionId = transition.id;
                tile.Save();
                if (Debug) UnityEngine.Debug.Log("Generated " + stairsType + " on " + tile.position);
            }
        }
    }
}