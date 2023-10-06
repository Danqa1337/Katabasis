using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class Location
{
    [SerializeField] private int _level;
    [SerializeField] private GenerationPresetName _generationPresetName;
    [SerializeField] private HyperstuctureName _hyperstuctureName;

    public readonly string Name;
    public readonly int Id;
    public readonly List<int> TransitionsIDs = new List<int>();
    public bool IsGenerated = false;

    private List<SimpleObjectData> _incomeingSimpleObjects = new List<SimpleObjectData>();
    private List<ComplexObjectData> _incomeingComplexObjects = new List<ComplexObjectData>();

    public bool IsCurrentLocation => Registers.GlobalMapRegister.CurrentLocation.Id == Id;
    public List<SimpleObjectData> IncomeingSimpleObjects { get => _incomeingSimpleObjects; }
    public List<ComplexObjectData> IncomeingComplexObjects { get => _incomeingComplexObjects; }
    public int Level { get => _level; }
    public GenerationPresetName GenerationPreset { get => _generationPresetName; }
    public HyperstuctureName HyperstuctureName { get => _hyperstuctureName; }

    public Location(int id, int level, GenerationPresetName generationPreset, HyperstuctureName hyperstuctureName)
    {
        Id = id;
        _level = level;
        _generationPresetName = generationPreset;
        Name = GetName();
        _hyperstuctureName = hyperstuctureName;
        TransitionsIDs = new List<int>();
    }

    private string GetName()
    {
        return KatabasisUtillsClass.IntToRomanNumeral(Id + 1) + " - " + LocalizationManager.GetString("Generation presets", GenerationPreset.ToString());
    }

    public override string ToString()
    {
        var str = "Location " + Name + " Id: " + Id + " Transitions: ";
        foreach (var id in TransitionsIDs)
        {
            str += " \n " + Registers.GlobalMapRegister.GetTransition(id).ToString() + " \n ";
        }

        return str;
    }

    public void AddIncomingObject(SimpleObjectData data)
    {
        _incomeingSimpleObjects.Add(data);
    }

    public void AddIncomingObject(ComplexObjectData data)
    {
        _incomeingComplexObjects.Add(data);
    }

    public void ClearIncomingObjects()
    {
        _incomeingComplexObjects = new List<ComplexObjectData>();
        _incomeingSimpleObjects = new List<SimpleObjectData>();
    }
}