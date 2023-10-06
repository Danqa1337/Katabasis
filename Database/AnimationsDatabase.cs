using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationsDatabase", menuName = "Databases/AnimationsDatabase")]
public class AnimationsDatabase : Database<AnimationsDatabase, SimpleObjectsTable, SimpleObjectsTable.Param, AnimationData>
{
    private Dictionary<SimpleObjectName, AnimationData> _dataByName;

    public static AnimationData GetAnimationData(SimpleObjectName simpleObjectName)
    {
        return instance._dataByName[simpleObjectName];
    }

    public override void StartUp()
    {
        _dataByName = new Dictionary<SimpleObjectName, AnimationData>();
        ReadPersistentList();
        foreach (var item in _persistentDataList)
        {
            item.animatorController = Resources.Load<AnimatorController>("AnimatorControllers/" + item.SimpleObjectName + "Controller");
            _dataByName.Add(item.SimpleObjectName, item);
        }
    }

    protected override void ProcessParam(SimpleObjectsTable.Param param)
    {
        _persistentDataList.Add(new AnimationData(param.enumName.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>()));
    }
}

[Serializable]
public class AnimationData
{
    public readonly SimpleObjectName SimpleObjectName;
    [NonSerialized] public AnimatorController animatorController;

    public AnimationData(SimpleObjectName simpleObjectName)
    {
        SimpleObjectName = simpleObjectName;
    }
}