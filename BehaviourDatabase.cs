using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
[CreateAssetMenu(fileName = "BehavioursDatabase", menuName = "Databases/BehavioursDatabase")]

public class BehaviourDatabase : Database<BehaviourDatabase, BehaviourDatabase, BehaviourDatabase, BehaviourDatabase>
{
    private static Dictionary<AiBehaviourName, Behaviours.IAIBehaviourStruct> _behavioursByName;

    public static Behaviours.IAIBehaviourStruct GetBehaviour(AiBehaviourName aiBehaviourName)
    {
        if (_behavioursByName.ContainsKey(aiBehaviourName))
        {
            return _behavioursByName[aiBehaviourName];
        }
        else
        {
            throw new System.Exception("Behaviour not found: " + aiBehaviourName);
        }
    }
    public override void StartUp()
    {
        var behaviourStructs = typeof(Behaviours).GetNestedTypes();
        _behavioursByName = new Dictionary<AiBehaviourName, Behaviours.IAIBehaviourStruct>();
        foreach (var behaviourStruct in behaviourStructs)
        {
            if (typeof(Behaviours.IAIBehaviourStruct).IsAssignableFrom(behaviourStruct) && behaviourStruct != typeof(Behaviours.IAIBehaviourStruct))
            {
                var behaviourInstance = Activator.CreateInstance(behaviourStruct) as Behaviours.IAIBehaviourStruct;
                _behavioursByName.Add(behaviourInstance.behaviourName, behaviourInstance);
            }
        }
    }

    protected override void ProcessParam(BehaviourDatabase param)
    {
        throw new NotImplementedException();
    }
}
