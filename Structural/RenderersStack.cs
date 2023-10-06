using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public abstract class RenderersStack : MonoBehaviour
{
    public List<Image> renderers;
    public Color color
    {
        set
        {
            foreach (var item in renderers)
            {


                item.material.SetColor("TintColor", value);
                item.color = value;

            }
        }
        get
        {


            return renderers[0].material.GetColor("TintColor");


        }
    }

    public void Clear()
    {
        foreach (var item in renderers)
        {
            item.sprite = null; item.color = Color.clear;
            item.transform.localPosition = Vector3.zero;
        }
    }
}
