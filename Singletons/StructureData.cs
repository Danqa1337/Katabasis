using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class StructureData : DeepClonable
{
    public readonly StructureName StructureName;
    [SerializeField] public StructureMap structureMap;
    [SerializeField] public ComplexObjectName[] CreatureNames = new ComplexObjectName[10];
    [SerializeField] public EnumFilter<TemplateState> TemplateStateFilter;



    public int Width => structureMap.Width;
    public int Height => structureMap.Height;
    public int2[] spawnPoints => structureMap.SpawnPoints;
    public TileTemplate[] Map => structureMap.Map;
    public StructureData(StructureName structureName)
    {
        StructureName = structureName;
    }
    public StructureData Turn()
    {
        var turnedStructure = this.DeepClone<StructureData>();
        turnedStructure.structureMap = structureMap.Turn();
        return turnedStructure;
    }
    public StructureData Mirror()
    {
        return this;
    }
    public int GetMaxExtent()
    {
        return Mathf.Max(Width, Height);
    }
}




