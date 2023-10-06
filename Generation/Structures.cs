using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GenerationModules
{
    public class Structures : GenerationModule
    {
        [SerializeField] private int _structuresNum;

        public override void Generate(GenerationData generationData)
        {
            var structureNames = new List<string>();
            var structureList = new List<StructureData>();

            for (int i = 0; i < _structuresNum; i++)
            {
                var structureData = StructuresDatabase.GetRandomStructureData(generationData.Location.GenerationPreset, generationData.Location.Level, 64);

                structureList.Add(structureData);
            }
            structureList.Shuffle();
            UnityEngine.Debug.Log(structureList.Count + " rooms");

            for (int i = 0; i < _structuresNum; i++)
            {
                var freeTiles = GetAllMapTiles().ToList();
                var leftCorner = freeTiles.RandomItem();

                foreach (var structure in structureList)
                {
                    var newBuiltStructure = StructureBuilder.TryToFitStructure(structure, leftCorner);
                    if (newBuiltStructure != null)
                    {
                        structureNames.Add(structure.StructureName.ToString());
                        break;
                    }
                }
                structureList.Shuffle();
            }

            if (Debug)
            {
                var message = structureNames.Count + " rooms spawned ";
                foreach (var name in structureNames)
                {
                    message += name + ", ";
                }
                UnityEngine.Debug.Log(message);
            }
        }
    }
}