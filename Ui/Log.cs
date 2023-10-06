using UnityEngine;

public class Log : MonoBehaviour
{
    public RectTransform panel;
    private Vector2 defaultScale;
    public bool expanded;
    public void Start()
    {
        defaultScale = panel.sizeDelta;
        expanded = false;
    }
    public void toggle()
    {
        if (expanded)
        {
            MinimizeLog();
        }
        else
        {
            MaximizeLog();
        }
    }
    public void MaximizeLog()
    {
        expanded = true;
        panel.sizeDelta = new Vector2(12, defaultScale.y);
        transform.localScale = new Vector3(-1, 1, 1);
    }
    public void MinimizeLog()
    {
        expanded = false;
        panel.sizeDelta = defaultScale;
        transform.localScale = new Vector3(1, 1, 1);
    }
}
