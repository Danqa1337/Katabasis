using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class EnumsUpdater
{
    private const string _enumFolderPath = "Assets/Scripts/Enums/";
    public static bool UpdateEnum(string enumName, string[] values)
    {
        var path = _enumFolderPath + enumName + ".cs";
        var fileContentsBuilder = new StringBuilder(10000);
        fileContentsBuilder.Append("public enum " + enumName + " \n{\nNull, \nAny, \n");
        
        foreach (var item in values)
        {
            fileContentsBuilder.Append(item + ", \n");
        }
        fileContentsBuilder.Append("\n}");
        var fileContents = fileContentsBuilder.ToString();

        if (!File.Exists(path) || File.ReadAllText(path) != fileContents)
        {
            File.WriteAllText(path, fileContents);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
            Debug.Log("Enum on path " + path + " updated");
            return true;
        }
        return false;
    }
}
