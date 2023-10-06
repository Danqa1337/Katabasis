using Gods;
using UnityEngine;
using UnityEngine.UI;

public class GodMiniIcon : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TrailRenderer _trailRenderer;
    private int _godIndex;

    public void Clear()
    {
        _image.color = Color.clear;
    }

    public void Draw(God god)
    {
        if (god is RandomizedGod)
        {
            Draw(god as RandomizedGod);
        }
        else
        {
            _godIndex = god.Index;
            _image.sprite = IconDataBase.GetGodIcon(god.GodArchetype);
        }
    }

    public void Draw(RandomizedGod randomizedGod)
    {
        _godIndex = randomizedGod.Index;
        _image.color = randomizedGod.IconData.MiddleGodIconPart.Value;
        _trailRenderer.endColor = randomizedGod.IconData.MiddleGodIconPart.Value;
    }
}