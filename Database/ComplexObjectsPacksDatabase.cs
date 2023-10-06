using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ComplexObjectsPacksDatabase", menuName = "Databases/ComplexObjectsPacksDatabase")]
public class ComplexObjectsPacksDatabase : Database<ComplexObjectsPacksDatabase, ComplexObjectsPacksTable, ComplexObjectsPacksTable.Param, SpawnData<PackData<ComplexObjectData>>>
{
    private Dictionary<PackName, PackData<ComplexObjectData>> _packsByName;
    protected override string enumName { get => "PackName"; }
    public override void StartUp()
    {
        ReadPersistentList();
        _packsByName = new Dictionary<PackName, PackData<ComplexObjectData>>();
        foreach (var data in _persistentDataList)
        {
            if (data.Data.PackName != PackName.Any)
            {
                _packsByName.Add(data.Data.PackName, data.Data);
            }
        }
    }

    protected override void ProcessParam(ComplexObjectsPacksTable.Param param)
    {
        var pack = new PackData<ComplexObjectData>(param.enumName.DecodeCharSeparatedEnumsAndGetFirst<PackName>());
        foreach (var item in param.members.Split(','))
        {
            pack.members.Add(ObjectDataFactory.GetComplexObjectDataFromString(item));
        }
        var packSpawnData = new SpawnData<PackData<ComplexObjectData>>(pack, new EnumFilter<Biome>(param.biomes) , param.normalDepthStart, param.normalDepthEnd, param.baseSpawnChance);

        _persistentDataList.Add(packSpawnData);
    }
    public static PackData<ComplexObjectData> GetPackData(PackName packName)
    {
        if (instance._packsByName.ContainsKey(packName))
        {
            return instance._packsByName[packName].DeepClone<PackData<ComplexObjectData>>();
        }
        throw new NullReferenceException("there is no such pack " + packName);
    }
    public static PackData<ComplexObjectData> GetRandomPackData(int level, Biome biome)
    {
        return instance._persistentDataList.GetRandomSpawnData(level, biome: biome).Data;
    }
    

}
