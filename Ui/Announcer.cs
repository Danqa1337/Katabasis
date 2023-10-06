using System.Collections;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class Announcer : Singleton<Announcer>
{
    public Canvas canvas;
    public Label messageLabel;
    private Vector2 _initialSize;
    private RectTransform _rectTransform;
    [SerializeField] private float _shownigTimeSeconds = 4f;
    private const int _frameCount = 16;

    private void OnEnable()
    {
        LocationEnterManager.OnMovingToLocationCompleted += AnnounceLocationEnter;
    }

    private void OnDisable()
    {
        LocationEnterManager.OnMovingToLocationCompleted -= AnnounceLocationEnter;
    }

    private void Awake()
    {
        canvas.enabled = false;
        messageLabel = GetComponentInChildren<Label>();
        _rectTransform = messageLabel.GetComponent<RectTransform>();
        _initialSize = new Vector2(1400, 140);
    }

    public static void AnnounceLocationEnter(Location location)
    {
        Announce(LocalizationManager.GetString("LocationAnnounce") + " " + location.Name);
    }

    public static void Announce(string message)
    {
        instance.StartCoroutine(instance.ShowMessage(message));
    }

    private IEnumerator ShowMessage(string message)
    {
        instance.canvas.enabled = true;
        instance._rectTransform.sizeDelta = new Vector2(0, instance._initialSize.y);
        SoundSystem.PlaySound(SoundName.OpenScroll);

        for (int i = 0; i < _frameCount; i++)
        {
            instance._rectTransform.sizeDelta = new Vector2(instance._initialSize.x / _frameCount * i, instance._initialSize.y);
            yield return new WaitForSeconds(0.01f);
        }

        messageLabel.SetText(message);

        yield return new WaitForSeconds(_shownigTimeSeconds);

        messageLabel.SetText("");
        for (int i = 0; i < _frameCount; i++)
        {
            _rectTransform.sizeDelta = new Vector2(_initialSize.x - _initialSize.x / _frameCount * i, _initialSize.y);
            yield return new WaitForSeconds(0.01f);
        }

        canvas.enabled = false;
    }
}