using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteCollectionsDatabase", menuName = "Databases/SpriteCollectionsDatabase")]
public class SpriteCollectionsDatabase : Database<SpriteCollectionsDatabase, SimpleObjectsTable, SimpleObjectsTable.Param, ObjectSpritesCollection>
{
    private Dictionary<SimpleObjectName, ObjectSpritesCollection> _spriteCollectionsByName;
    public static List<ObjectSpritesCollection> SpritesCollections => instance._persistentDataList;
    public override void StartUp()
    {
        Reimport();
        instance._spriteCollectionsByName = new Dictionary<SimpleObjectName, ObjectSpritesCollection>();
        foreach (var item in _persistentDataList)
        {
            _spriteCollectionsByName.Add(item.name, item);
        }
    }
    protected override void WritePersistentList()
    {
        
    }
    protected override void ProcessParam(SimpleObjectsTable.Param param)
    {
        _persistentDataList.Add(new ObjectSpritesCollection(param.enumName.DecodeCharSeparatedEnumsAndGetFirst<SimpleObjectName>()));
    }
 
    public static ObjectSpritesCollection GetSpriteCollection(SimpleObjectName name)
    {
        if (name == SimpleObjectName.Null)
        {
            return null;
        }

        if (Application.isPlaying)
        {
            if (instance._spriteCollectionsByName.ContainsKey(name))
            {
                var collection = instance._spriteCollectionsByName[name];
                return collection;
            }
            throw new NullReferenceException("there is no such sprites collection " + name);
        }
        else
        {
            foreach (var obj in instance._persistentDataList)
            {
                if (obj.name == name)
                {
                    return obj;
                }
            }
            throw new NullReferenceException("there is no such sprites collection " + name);
        }
    }
}
