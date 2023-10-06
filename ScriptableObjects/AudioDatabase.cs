using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum SoundName
{
    Null,
    Click,

    Woosh,
    HitSoft,
    StepSoft,
    HitHard,
    StepLiquid,
    StepHard,
    StepPlant,
    Splash,
    Death,
    Eat,
    PressurePlate,
    DoorOpen,
    DoorClose,
    RestorePart,
    LoosePart,
    Lock,
    Unlock,
    Explosion,
    Eager,
    Decline,
    Anger,
    BreakStone,
    BreakOrganic,
    BreakWood,
    BreakCeramic,
    BreakPlants,
    OpenScroll,
    FindStairs,
    AmbientSounds,
}

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Databases/AudioDatabase")]
public class AudioDatabase : Database<AudioDatabase, AudioDatabase, AudioDatabase, AudioDatabase>
{
    [SerializeField] public EventReference EventNotFound;
    [SerializeField] public EventReference Click;
    [SerializeField] public EventReference StepSoft;
    [SerializeField] public EventReference Woosh;
    [SerializeField] public EventReference HitSoft;
    [SerializeField] public EventReference HitHard;
    [SerializeField] public EventReference StepLiquid;
    [SerializeField] public EventReference StepHard;
    [SerializeField] public EventReference StepPlant;
    [SerializeField] public EventReference Splash;
    [SerializeField] public EventReference Death;
    [SerializeField] public EventReference Eat;
    [SerializeField] public EventReference PressurePlate;
    [SerializeField] public EventReference DoorOpen;
    [SerializeField] public EventReference DoorClose;
    [SerializeField] public EventReference RestorePart;
    [SerializeField] public EventReference LoosePart;
    [SerializeField] public EventReference Lock;
    [SerializeField] public EventReference Unlock;
    [SerializeField] public EventReference Explosion;
    [SerializeField] public EventReference Eager;
    [SerializeField] public EventReference Decline;
    [SerializeField] public EventReference Anger;
    [SerializeField] public EventReference BreakStone;
    [SerializeField] public EventReference BreakOrganic;
    [SerializeField] public EventReference BreakWood;
    [SerializeField] public EventReference BreakCeramic;
    [SerializeField] public EventReference BreakPlants;
    [SerializeField] public EventReference OpenScroll;
    [SerializeField] public EventReference FindStairs;
    [SerializeField] public EventReference AmbientSounds;

    private Dictionary<SoundName, EventReference> _eventsByName;

    public override void StartUp()
    {
        _eventsByName = new Dictionary<SoundName, EventReference>();
        var eventRefs = instance.GetType().GetFields();
        var soundNameValues = Enum.GetValues(typeof(SoundName));

        _eventsByName.Add(SoundName.Null, instance.EventNotFound);
        foreach (var item in soundNameValues)
        {
            bool found = false;
            foreach (var reference in eventRefs)
            {
                if (reference.Name == item.ToString())
                {
                    found = true;
                    _eventsByName.Add((SoundName)item, (EventReference)reference.GetValue(instance));
                    break;
                }
            }
            if (!found)
            {
                UnityEngine.Debug.Log("Audio event not found for " + item.ToString());
            }
        }
    }

    public static EventReference GetAudioEvent(SoundName soundName)
    {
        if (instance._eventsByName.ContainsKey(soundName))
        {
            return instance._eventsByName[soundName];
        }
        else
        {
            UnityEngine.Debug.Log("AudioEvent for " + soundName + " not found");
            return instance.EventNotFound;
        }
    }

    protected override void ProcessParam(AudioDatabase param)
    {
        
    }
}