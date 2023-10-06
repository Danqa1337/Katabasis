using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TableImporter
{
    public static TTable FindTable<TTable>() where TTable : ScriptableObject
    {
        var tableType = typeof(TTable);
        var tables = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Excel/" + (typeof(TTable).ToString() + ".asset"));
        if (tables.Length == 0)
        {
            throw new System.Exception("Can not find " + tableType.ToString());
        }
        if (tables.Length > 1)
        {
            UnityEngine.Debug.Log("Located " + tables.Length + " instances of " + tableType.ToString());
        }

        return tables[tables.Length-1] as TTable;
    }

    public static TParam[] GetParams<TTable, TParam>(TTable table)
    {
        var result = new List<TParam>();

        var @params = (List<TParam>)table.GetType().GetField("param").GetValue(table);
        result.AddRange(@params);
        result.Remove(result[0]);
        return result.ToArray();
    }
}
