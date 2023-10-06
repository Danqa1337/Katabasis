using System;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using UnityWeld.Binding;

[Binding]
public class PlayerStats : MonoBehaviour, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    [Binding]
    public int Str { get => Player.PlayerEntity != Entity.Null ? Player.CreatureComponent.str : 0; }

    [Binding]
    public int Agl { get => Player.PlayerEntity != Entity.Null ? Player.CreatureComponent.agl : 0; }

    [Binding]
    public int MaxSquadSize { get => Registers.IsInit ? Registers.StatsRegister.MaxSquadSize : 0; }

    [Binding]
    public int MaxGnosis { get => Registers.IsInit ? Registers.StatsRegister.MaxGnosis : 0; }

    [Binding]
    public int CurrentGnosis { get => Registers.IsInit ? Registers.StatsRegister.CurrentGnosis : 0; }

    [Binding]
    public int Level { get => Registers.IsInit ? Registers.StatsRegister.Level : 0; }

    [Binding]
    public int CurrentXP { get => Registers.IsInit ? Registers.StatsRegister.CurrentXP : 0; }

    [Binding]
    public int FreeStatPoints { get => Registers.IsInit ? Registers.StatsRegister.FreeStatPoints : 0; }

    [Binding]
    public int XPToNextLevel { get => Registers.IsInit ? Registers.StatsRegister.XPToNextLevel : 0; }

    [Binding]
    public int Ticks { get => Registers.IsInit ? Registers.StatsRegister.Ticks : 0; }

    private void OnEnable()
    {
        Registers.OnLoaded += OnRegistersLoaded;
    }

    private void OnDisable()
    {
        Registers.OnLoaded -= OnRegistersLoaded;
    }

    private void OnRegistersLoaded()
    {
        Registers.StatsRegister.OnStatChanged += OnStatChanged;
        foreach (var item in Enum.GetValues(typeof(StatName)))
        {
            InvokePropertyChange(item.ToString());
        }
    }

    private void OnStatChanged(StatName statName, int value)
    {
        InvokePropertyChange(statName.ToString());
    }

    private void InvokePropertyChange(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: propertyName));
    }
}