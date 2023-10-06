using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Locations;

public class GlobalMapGenerator : Singleton<GlobalMapGenerator>
{
    public int LakeDepth;
    public int PitDepth;
    public float branchingChance;
    public int NumberOfLayers;

    [ContextMenu("Generate")]
    public GlobalMapRegister Generate()
    {
        Debug.Log("Generating Dungeon Structure");

        var transitions = new List<Transition>();
        var locations = new List<Location>();

        for (int i = 0; i < NumberOfLayers; i++)
        {
            GenerationPresetName preset = GenerationPresetName.Dungeon;
            if (i == LakeDepth) preset = GenerationPresetName.Lake;
            var newLocation = CreateNewLocation(preset, i);
        }

        for (int j = 0; j < NumberOfLayers - 1; j++)
        {
            var locationA = locations[j];
            var locationB = locations[j + 1];

            CreateTransition(locationA, locationB);
        }
        var pit = GeneratePit();
        var arena = GenerateArena();

        return new GlobalMapRegister(locations, transitions, pit, arena, NumberOfLayers);
        // DungeonStructureVisualizer.instance.Visualize();

        Transition CreateTransition(Location locationA, Location locationB,
            bool entranceExposed = true,
            bool exitExposed = true,
            bool entranceOpened = true,
            bool exitOpened = true)
        {
            var transition = new Transition(locationA.Id, locationB.Id, exitOpened, entranceOpened, entranceExposed, exitExposed);
            transition.id = transitions.Count;
            locationA.TransitionsIDs.Add(transition.id);
            locationB.TransitionsIDs.Add(transition.id);
            transitions.Add(transition);
            return transition;
        }

        Location GeneratePit()
        {
            var location = CreateNewLocation(GenerationPresetName.Pit, 5);

            CreateTransition(locations[5], location, entranceExposed: true);
            return location;
        }
        Location GenerateArena()
        {
            return CreateNewLocation(GenerationPresetName.Arena, 1);
        }

        Location CreateNewLocation(GenerationPresetName preset, int level)
        {
            var hyperStruct = level == 3 ? HyperstuctureName.Town : HyperstuctureName.Null;
            var newLocation = new Location(locations.Count, level, preset, hyperStruct);

            locations.Add(newLocation);
            return newLocation;
        }
    }
}