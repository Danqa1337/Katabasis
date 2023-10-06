using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerSettings", menuName = "PlayerSettings")]
public class PlayerSettings : SingletonScriptableObject<PlayerSettings>
{
    public float cameraMoveSens = 1;
    public float zoomSens = 1;
    public float incomingDamageMultipler = 1;
    public float volumeAmbient;
    public float volumeUi;
    public float volumeMusic;
}