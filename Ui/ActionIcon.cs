using UnityEngine;
using UnityEngine.UI;

public class ActionIcon : KatabasisSelectable
{
    [SerializeField] private Image _image;

    public void DrawAction(ControllerActionName controllerActionName)
    {
        ClearEvents();
        _image.sprite = IconDataBase.GetActionIcon(controllerActionName);
    }
}