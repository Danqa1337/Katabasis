using Assets.Scripts.Singletons;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New ColorScheme", menuName = "Databases")]
public class ColorScheme : SingletonScriptableObject<ColorScheme>
{
    public Color AnatomyColorLow;
    public Color AnatomyColorHigh;
    public Color AnatomyColorIntact;
    [Header("AnatomyPart")]
    public ColorBlock AnatomyPartColorBlock;
    [Header("Button")]
    public ColorBlock ButtonsColorBlock;
    public Color PanelColor;
    public Color TextColor;

    private void OnValidate()
    {
        LoadValues();
    }

    [ContextMenu("Submit")]
    public void LoadValues()
    {
#if UNITY_EDITOR
        var buttons = FindObjectsOfType<Button>();
        foreach (var item in buttons)
        {
            item.colors = ButtonsColorBlock;
        }

        if (Application.isPlaying && ColorSchemeViewModel.instance != null)
        {
            foreach (var field in instance.GetType().GetFields())
            {
                var viewModelProperty = typeof(ColorSchemeViewModel).GetProperty(field.Name);
                if (viewModelProperty != null)
                {
                    viewModelProperty.SetValue(ColorSchemeViewModel.instance, field.GetValue(instance));
                    ColorSchemeViewModel.instance.InvokePropertyChange(viewModelProperty.Name);
                }
            }
        }
        var panels = FindObjectsOfType<Panel>().Where(p => p.GetComponent<OverrideColorScheme>() == null);
        foreach (var item in panels)
        {
            if(item.GetComponent<Image>() == null)
            {
                Debug.Log(item.gameObject.name);
            }
            if (!item.overrideCollorScheme)
            {
                item.GetComponent<Image>().color = PanelColor;
                UnityEditor.EditorUtility.SetDirty(item);
            }
        }
        foreach (var item in FindObjectsOfType<TextMeshProUGUI>().Where(p => p.GetComponent<OverrideColorScheme>() == null))
        {
            item.color = TextColor;
            UnityEditor.EditorUtility.SetDirty(item);

        }
        AssetDatabase.SaveAssets();
#endif
    }
}