using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public enum LocalizationVariant
{
    English,
    Russian,
}

public static class LocalizationManager
{
    private const string _simpleDescriptionsTableName = "SimpleObjectsDescriptions";
    private const string _complexDescriptionsTableName = "ComplexObjectsDescriptions";
    private const string _simpleNamesTableName = "SimpleObjectsNames";
    private const string _complexNamesTableName = "ComplexObjectsNames";
    public static string GetString(string table, string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
    }
    public static string GetString(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("Main Table", key);
    }
    public static string GetDescription(SimpleObjectName key)
    {
        return GetDescriptionSimple(key.ToString());
    }
    public static string GetName(SimpleObjectName key)
    {
        return GetNameSimple(key.ToString());
    }
    public static string GetDescription(ComplexObjectName key)
    {
        return GetDescriptionComplex(key.ToString());
    }
    public static string GetName(ComplexObjectName key)
    {
        return GetNameComplex(key.ToString());
    }
    public static string GetDescriptionSimple(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(_simpleDescriptionsTableName, key);
    }
    public static string GetDescriptionComplex(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(_complexDescriptionsTableName, key);
    }
    public static string GetNameSimple(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(_simpleNamesTableName, key);
    }
    public static string GetNameComplex(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(_complexNamesTableName, key);
    }
    public static LocalizationVariant CurrentLocaliztion
    {
        get
        {
            if (LocalizationSettings.SelectedLocale.LocaleName == "English (en)")
            {
                return LocalizationVariant.English;
            }
            if (LocalizationSettings.SelectedLocale.LocaleName == "Russian (ru)")
            {
                return LocalizationVariant.Russian;
            }
            return LocalizationVariant.English;
        }
    }
    public static StringTable LoadTable(string name)
    {
        return Addressables.LoadAssetAsync<StringTable>(name).WaitForCompletion();
    }
    public static void SaveTable(StringTable stringTable)
    {
#if UNITY_EDITOR

        
        UnityEditor.EditorUtility.SetDirty(stringTable);
        UnityEditor.EditorUtility.SetDirty(stringTable.SharedData);

        UnityEditor.AssetDatabase.SaveAssets();
        Addressables.Release(stringTable);
#endif

    }
}