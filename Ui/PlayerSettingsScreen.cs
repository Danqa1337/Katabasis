using UnityEngine;
using UnityEngine.UI;

public class PlayerSettingsScreen : MonoBehaviour
{
    public Slider cameraMoveSensSlider;
    public Slider zoomSensSlider;
    public Slider IncomingDamgeMultiplerSlider;
    public Slider AmbientVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider UiVolumeSlider;

    private void OnEnable()
    {
        UiManager.OnHideCanvas += DoOnCanvasHide;
        UiManager.OnShowCanvas += DoOnCanvasShow;
    }

    private void OnDisable()
    {
        UiManager.OnHideCanvas -= DoOnCanvasHide;
        UiManager.OnShowCanvas -= DoOnCanvasShow;
    }

    private void Start()
    {
        AmbientVolumeSlider.onValueChanged.AddListener(delegate
        {
            AudioManager.ChangeUiVolume(AudioManager.VCAName.Ambient, AmbientVolumeSlider.value);
        });
        MusicVolumeSlider.onValueChanged.AddListener(delegate
        {
            AudioManager.ChangeUiVolume(AudioManager.VCAName.Music, MusicVolumeSlider.value);
        });
        UiVolumeSlider.onValueChanged.AddListener(delegate
        {
            AudioManager.ChangeUiVolume(AudioManager.VCAName.UI, UiVolumeSlider.value);
        });
        Load();
    }

    public void Load()
    {
        cameraMoveSensSlider.value = PlayerSettings.instance.cameraMoveSens;
        zoomSensSlider.value = PlayerSettings.instance.zoomSens;
        IncomingDamgeMultiplerSlider.value = PlayerSettings.instance.incomingDamageMultipler;
        AmbientVolumeSlider.value = PlayerSettings.instance.volumeAmbient;
        UiVolumeSlider.value = PlayerSettings.instance.volumeUi;
        MusicVolumeSlider.value = PlayerSettings.instance.volumeMusic;
    }

    public void Save()
    {
        PlayerSettings.instance.cameraMoveSens = cameraMoveSensSlider.value;
        PlayerSettings.instance.zoomSens = zoomSensSlider.value;
        PlayerSettings.instance.incomingDamageMultipler = IncomingDamgeMultiplerSlider.value;
        PlayerSettings.instance.volumeAmbient = AmbientVolumeSlider.value;
        PlayerSettings.instance.volumeUi = UiVolumeSlider.value;
        PlayerSettings.instance.volumeMusic = MusicVolumeSlider.value;
    }

    private void DoOnCanvasShow(UIName uIName)
    {
        if (uIName == UIName.Settings)
        {
            Load();
        }
    }

    private void DoOnCanvasHide(UIName uIName)
    {
        if (uIName == UIName.Settings)
        {
            Save();
        }
    }
}