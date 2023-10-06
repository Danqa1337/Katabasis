using System;
using Unity.Entities;
using System.Runtime.Serialization;

[Serializable]
public struct ComponentReferece<T> where T : IComponentData
{
    [UnityEngine.SerializeField]public readonly T Component;
    public readonly bool IsValid;
    public ComponentReferece(T component)
    {
        Component = component;
        IsValid = true;
    }

}



