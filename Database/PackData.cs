using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PackData<T> : DeepClonable where T : class
{
    public readonly PackName PackName;

    [SerializeField] public List<T> members = new List<T>();
    public int Count => members.Count;
    public PackData(PackName packName)
    {
        PackName = packName;
    }
}
