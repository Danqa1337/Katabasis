using System;
using UnityEngine;

public static class GnosisManager
{
    public static int CurrentGnosis { get => Registers.StatsRegister.CurrentGnosis; }
    public static int MaxGnosis { get => Registers.StatsRegister.CurrentGnosis; }

    public static void AddGnosis(int ammount)
    {
        var value = Mathf.Clamp(CurrentGnosis + ammount, 0, MaxGnosis);
        Registers.StatsRegister.SetStat(StatName.CurrentGnosis, value);
    }

    public static void RemoveGnosis(int ammount)
    {
        var value = Mathf.Clamp(CurrentGnosis - ammount, 0, MaxGnosis);
        Registers.StatsRegister.SetStat(StatName.MaxGnosis, value);
    }
}