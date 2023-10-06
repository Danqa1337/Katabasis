using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class ImageBlinkEffect : MonoBehaviour
{
    [SerializeField] private Color _blinkColor;
    [SerializeField] private float _blinkTime = 0.2f;
    [SerializeField] private int _loops = 3;
    private Image _image;
    private Color _defaultColor;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _defaultColor = _image.color;
    }

    public void Play()
    {
        _image.DOKill();
        _image.color = _defaultColor;
        _image.DOColor(_blinkColor, _blinkTime).From().SetLoops(_loops).OnComplete(delegate
        {
            _image.color = _defaultColor;
        });
    }
}