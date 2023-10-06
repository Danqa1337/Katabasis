using UnityEngine;
using UnityEngine.UI;
public class SquadmatePortrait : MonoBehaviour
{
    private RawImage rawImage;
    public RenderTexture RenderTexture
    {
        get
        {
            return rawImage.texture as RenderTexture;
        }
        set
        {
            rawImage.texture = value;
        }
    }
    private void Awake()
    {
        rawImage = GetComponentInChildren<RawImage>();
    }
}
