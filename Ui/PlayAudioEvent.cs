using UnityEngine;
using UnityEngine.UI;

public class PlayAudioEvent : MonoBehaviour
{
    public bool autoconnect;
    public SoundName soundName;

    private void Awake()
    {
        if (autoconnect)
        {
            GetComponent<Button>().onClick.AddListener(Play);
        }
    }
    public void Play()
    {
        AudioManager.PlayEvent(soundName);
    }
}
