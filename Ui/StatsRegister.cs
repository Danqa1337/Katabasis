using System;
using Unity.Entities;

public enum StatName
{
    Str,
    Agl,
    MaxSquadSize,
    CurrentGnosis,
    MaxGnosis,
    Level,
    FreeStatPoints,
    CurrentXP,
    XPToNextLevel,
    Piety,
    Ticks,
}

public class StatsRegister : IRegisterWithSubscription
{
    public event Action<StatName, int> OnStatChanged;

    private int _maxSquadSize = 3;
    private int _maxGnosis = 2000;
    private int _currentGnosis = 0;
    private int _level = 0;
    private int _currentXp = 0;
    private int _xPToNextLevel = 1;
    private int _freeStatPoints = 0;
    private int _ticks = 0;
    public int Str { get => Player.PlayerEntity != Entity.Null ? Player.CreatureComponent.str : 0; }
    public int Agl { get => Player.PlayerEntity != Entity.Null ? Player.CreatureComponent.agl : 0; }
    public int MaxSquadSize { get => _maxSquadSize; }
    public int MaxGnosis { get => _maxGnosis; }
    public int CurrentGnosis { get => _currentGnosis; }
    public int Level { get => _level; }
    public int CurrentXP { get => _currentXp; }
    public int FreeStatPoints { get => _freeStatPoints; }
    public int XPToNextLevel { get => _xPToNextLevel; }
    public int Ticks { get => _ticks; }

    public void SetStat(StatName statName, int value)
    {
        switch (statName)
        {
            case StatName.Str:
                SetStr(value);
                break;

            case StatName.Agl:
                SetAgl(value);
                break;

            case StatName.MaxSquadSize:
                _maxSquadSize = value;
                break;

            case StatName.CurrentGnosis:
                _currentGnosis = value;
                break;

            case StatName.MaxGnosis:
                _maxGnosis = value;
                break;

            case StatName.Level:
                _level = value;
                break;

            case StatName.FreeStatPoints:
                _freeStatPoints = value;
                break;

            case StatName.CurrentXP:
                _currentXp = value;
                break;

            case StatName.XPToNextLevel:
                _xPToNextLevel = value;
                break;

            case StatName.Ticks:
                _ticks = value;
                break;

            default:
                break;
        }
        OnStatChanged?.Invoke(statName, value);
    }

    public int GetStat(StatName statName)
    {
        return (int)GetType().GetProperty(statName.ToString()).GetValue(this);
    }

    public void OnEnable()
    {
        TimeController.OnTickStarted += OnTick;
    }

    public void OnDisable()
    {
        TimeController.OnTickStarted -= OnTick;
    }

    private void SetStr(int value)
    {
        var component = Player.CreatureComponent;
        component.str = value;
        Player.PlayerEntity.SetComponentData(component);
    }

    private void SetAgl(int value)
    {
        var component = Player.CreatureComponent;
        component.agl = value;
        Player.PlayerEntity.SetComponentData(component);
    }

    private void OnTick()
    {
        SetStat(StatName.Ticks, Ticks + 1);
    }
}