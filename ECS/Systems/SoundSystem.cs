using System.Collections.Generic;
using Unity.Entities;

[DisableAutoCreation]
public partial class SoundSystem : MySystemBase
{
    private static List<SoundAndTile> scheduledSounds = new List<SoundAndTile>();

    public static void ScheduleSound(SoundName soundName, TileData tileData)
    {
        scheduledSounds.Add(new SoundAndTile(soundName, tileData));
    }

    protected override async void OnUpdate()
    {
        foreach (var item in scheduledSounds)
        {
            if (item.TileData.visible)
            {
                PlaySound(item.SoundName);
            }
        }
        scheduledSounds.Clear();
    }

    public static void PlaySound(SoundName soundName)
    {
        AudioManager.PlayEvent(soundName);
    }

    private struct SoundAndTile
    {
        public SoundName SoundName;
        public TileData TileData;

        public SoundAndTile(SoundName soundName, TileData tileData)
        {
            SoundName = soundName;
            TileData = tileData;
        }
    }
}

[System.Serializable]
public struct ObjectSoundData : IComponentData
{
    public SoundName StepSound;
    public SoundName BreakSound;

    public ObjectSoundData(SimpleObjectsTable.Param param)
    {
        StepSound = param.stepSound.DecodeCharSeparatedEnumsAndGetFirst<SoundName>();
        BreakSound = param.breakSound.DecodeCharSeparatedEnumsAndGetFirst<SoundName>();
    }
}
[System.Serializable]
public struct CreatureSoundData : IComponentData
{
    public SoundName StepSound;
    public SoundName BreakSound;
}