using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(KatabasisSelectable))]
public class ToggleUiButton : MonoBehaviour
{
    public UIName menu;

    private void Awake()
    {
        GetComponent<KatabasisSelectable>().OnLeftClickEvent.AddListener(Click);
    }

    private void Click()
    {
        UiManager.ToggleUiCanvas(menu);
    }
}