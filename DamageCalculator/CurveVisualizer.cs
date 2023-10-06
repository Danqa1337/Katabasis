using System.Collections.Generic;
using UnityEngine;
public class CurveVisualizer : MonoBehaviour
{
    public Texture2D texture;
    public Color weightColor, elegancyColor, backGroundColor, gridColor;

    public List<Label> yCounters;
    public List<Label> xCounters;
    [Range(1, 10)] public int scaleX = 1, scaleY = 1;

    private List<Vector2> lastKeys;
    public void Clear()
    {

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, backGroundColor);
            }
        }
    }
    public void visualize(List<Vector2> keys, Color color)
    {
        lastKeys = keys;
        for (int i = 0; i < keys.Count; i++)
        {
            var item = keys[i];
            if (item.x < texture.width && item.y < texture.height)
            {
                if (i % 100 == 0) xCounters[i / 100].SetValue(item.x - 1);
                for (int y = 0; y < (int)(item.y * 2.5 * scaleY); y++)
                {
                    texture.SetPixel((int)(item.x * 2.5 * scaleX), (y), color);
                }

            }
            else break;

        }


        drawGrid();
        texture.Apply();


    }
    void DrawLastCurve()
    {
        visualize(lastKeys, weightColor);
    }
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {


            //Clear();
            //DrawLastCurve();
        }
    }
    private void drawGrid()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                if ((x % 25 == 0 || y % 25 == 0))
                {


                    texture.SetPixel(x, y, gridColor);
                }
            }
        }
    }
}
