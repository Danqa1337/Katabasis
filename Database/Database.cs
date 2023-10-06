using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public abstract class Database<Tself, Ttable, Tparam, Tdata> : SingletonScriptableObject<Tself>, IStartUp
    where Tself : Database<Tself, Ttable, Tparam, Tdata>
    where Ttable : ScriptableObject
    where Tparam : class
{
    [SerializeField]
    protected List<Tdata> _persistentDataList;

    protected StringTable _descriptionsStringTableRU;
    protected StringTable _namesStringTableRU;
    protected StringTable _descriptionsStringTableEN;
    protected StringTable _namesStringTableEN;
    protected virtual string enumName { get => ""; }
    protected virtual string stringTableName { get => ""; }
    public static List<Tdata> PersistentDataList => instance._persistentDataList;
    protected virtual string _binaryPath => Application.dataPath + "/Resources/DatabaseBinaryFiles/" + GetType() + "BinaryData.bytes";

    protected abstract void ProcessParam(Tparam param);

    protected Tparam[] GetParams()
    {
        return TableImporter.GetParams<Ttable, Tparam>(TableImporter.FindTable<Ttable>());
    }

    public virtual void Reimport()
    {
        StartReimport();
        if (UpdateEnum()) return;
        LoadStringTables();
        _persistentDataList = new List<Tdata>();
        var parameters = GetParams();

        foreach (var param in parameters)
        {
            ProcessParam(param);
        }

        SaveStringTables();
        WritePersistentList();
        EndReimport();
    }

    protected virtual void StartReimport()
    {
        Debug.Log("Reimporting " + GetType());
    }

    protected virtual void EndReimport()
    {
        Debug.Log("Reimporting " + GetType() + " complete!");
    }

    protected virtual void WritePersistentList()
    {
        BinarySerializer.Write(_persistentDataList, _binaryPath);
    }

    protected virtual void ReadPersistentList()
    {
        _persistentDataList = BinarySerializer.Read<List<Tdata>>(_binaryPath);
    }

    protected virtual bool UpdateEnum()
    {
        if (enumName == "") return false;
        var names = new List<string>();
        var enumNameField = typeof(Tparam).GetField("enumName");

        foreach (var param in GetParams())
        {
            var name = (string)enumNameField.GetValue(param);
            names.Add(name);
        }
        return EnumsUpdater.UpdateEnum(enumName, names.ToArray());
    }

    protected virtual void LoadStringTables()
    {
        if (stringTableName != "")
        {
            _descriptionsStringTableEN = LocalizationManager.LoadTable(stringTableName + "Descriptions_en");
            _descriptionsStringTableRU = LocalizationManager.LoadTable(stringTableName + "Descriptions_ru");
            _namesStringTableEN = LocalizationManager.LoadTable(stringTableName + "Names_en");
            _namesStringTableRU = LocalizationManager.LoadTable(stringTableName + "Names_ru");

            _descriptionsStringTableEN.Clear();
            _descriptionsStringTableRU.Clear();
            _namesStringTableEN.Clear();
            _namesStringTableRU.Clear();
        }
    }

    protected virtual void SaveStringTables()
    {
#if UNITY_EDITOR
        if (stringTableName != "")
        {
            LocalizationManager.SaveTable(_descriptionsStringTableEN);
            LocalizationManager.SaveTable(_descriptionsStringTableRU);
            LocalizationManager.SaveTable(_namesStringTableEN);
            LocalizationManager.SaveTable(_namesStringTableRU);
        }
#endif
    }

    public abstract void StartUp();
}

public interface IStartUp
{
    public abstract void StartUp();
}