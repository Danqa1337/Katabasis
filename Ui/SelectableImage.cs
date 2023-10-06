using UnityEngine;
using UnityEngine.UI;

public class SelectableImage : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = 0.5f;
    }
}