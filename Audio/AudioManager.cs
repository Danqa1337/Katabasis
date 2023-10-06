using FMOD.Studio;
using FMODUnity;

public class AudioManager : Singleton<AudioManager>
{
    public enum VCAName
    {
        Ambient,
        Music,
        UI,
    }

    public bool debug;
    private VCA AmbientVCA;
    private VCA UIVCA;
    private VCA MusicVCA;

    public void Start()
    {
        AmbientVCA = RuntimeManager.GetVCA("vca:/Ambient");
        UIVCA = RuntimeManager.GetVCA("vca:/UI");
        MusicVCA = RuntimeManager.GetVCA("vca:/Music");
    }

    public static void PlayEvent(EventReference eventReference)
    {
        RuntimeManager.PlayOneShot(eventReference);
    }

    public static void PlayEvent(SoundName soundName)
    {
        if (instance.debug) UnityEngine.Debug.Log("Playing " + soundName);
        RuntimeManager.PlayOneShot(AudioDatabase.GetAudioEvent(soundName));
    }

    public static void ChangeUiVolume(VCAName vCAName, float value)
    {
        switch (vCAName)
        {
            case VCAName.Ambient:
                instance.AmbientVCA.setVolume(value);
                break;

            case VCAName.Music:
                instance.MusicVCA.setVolume(value);
                break;

            case VCAName.UI:
                instance.UIVCA.setVolume(value);
                break;

            default:
                break;
        }
    }
}