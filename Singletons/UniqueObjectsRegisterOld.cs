using System.Collections.Generic;
using System;

[System.Serializable]
public class UniqueObjectsRegister
{
    public SerializableDictionary<SimpleObjectName, UniqueObjectData> _uniqueSimpleObjects;
    public SerializableDictionary<ComplexObjectName, UniqueObjectData> _uniqueComplexObjects;

    public UniqueObjectsRegister()
    {
        _uniqueSimpleObjects = new SerializableDictionary<SimpleObjectName, UniqueObjectData>();
        _uniqueComplexObjects = new SerializableDictionary<ComplexObjectName, UniqueObjectData>();
        foreach (SimpleObjectName name in Enum.GetValues(typeof(SimpleObjectName)))
        {
            if ((int)name > 1)
            {
                if (SimpleObjectsDatabase.GetObjectData(name, true).tagElements.Contains(new TagBufferElement(Tag.Unique)))
                {
                    _uniqueSimpleObjects.Add(name, new UniqueObjectData());
                }
            }
        }
        foreach (ComplexObjectName name in Enum.GetValues(typeof(ComplexObjectName)))
        {
            if ((int)name > 1)
            {
                if (ComplexObjectsDatabase.GetObjectData(name, true).Body.tagElements.Contains(new TagBufferElement(Tag.Unique)))
                {
                    _uniqueComplexObjects.Add(name, new UniqueObjectData());
                }
            }
        }
    }

    public bool Contains(SimpleObjectName itemName)
    {
        return _uniqueSimpleObjects.ContainsKey(itemName);
    }

    public void Set(SimpleObjectName itemName, bool spawned, bool alive)
    {
        var data = _uniqueSimpleObjects[itemName];
        data.alive = alive;
        data.spawned = spawned;
    }

    public UniqueObjectData Get(SimpleObjectName itemName)
    {
        return _uniqueSimpleObjects[itemName];
    }
}

[System.Serializable]
public class UniqueObjectData
{
    public bool alive;
    public bool spawned;
}