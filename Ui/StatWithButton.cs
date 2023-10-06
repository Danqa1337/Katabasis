using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatWithButton : Stat
{
    [SerializeField] private StatName _statName;
    [SerializeField] private UiFlicker _increaseFlicker;
    [SerializeField] private KatabasisButton _increaseStatButton;

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
        Registers.StatsRegister.OnStatChanged += ListenPointsChange;
    }

    private void ListenPointsChange(StatName statName, int value)
    {
        if (statName == StatName.FreeStatPoints)
        {
            _increaseStatButton.interactable = value > 0;
            _increaseFlicker.SetActive(value > 0);
        }
    }

    public void IncreaseStat()
    {
        Registers.StatsRegister.SetStat(_statName, Registers.StatsRegister.GetStat(_statName));
    }
}