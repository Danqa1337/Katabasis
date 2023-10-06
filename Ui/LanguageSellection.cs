using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSellection : MonoBehaviour
{
    private void Start()
    {
        //if (LocalizationSettings.InitializationOperation.IsValid())
        //{ 
        //    LocalizationSettings.InitializationOperation.WaitForCompletion();
        //} 
        ChangeLocale(1);
    }

    public void ChangeLocale(int value)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value];
        Debug.Log("Selected " + LocalizationSettings.SelectedLocale.LocaleName);
    }
}