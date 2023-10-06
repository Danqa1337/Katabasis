using System;
using UnityEngine;

public static class PlayerXPHandler
{
    private const int baseXPLevelCost = 100;

    public static event Action<int> OnXPChanged;

    public static event Action<int> OnStat;

    private static int GetNextLevelCost(int currentLevel)
    {
        return (int)(baseXPLevelCost + baseXPLevelCost * 1.5f * (currentLevel - 1));
    }

    public static void AddXP(int xp)
    {
        var currentXp = Registers.StatsRegister.CurrentXP + xp;
        var currentLevel = Registers.StatsRegister.Level;
        var freeStatPoints = Registers.StatsRegister.FreeStatPoints;
        var nextLevelCost = GetNextLevelCost(currentLevel);
        var levelIncreased = false;

        while (currentXp >= nextLevelCost)
        {
            currentXp -= GetNextLevelCost(currentLevel);
            currentLevel++;
            freeStatPoints += 4;
            levelIncreased = true;
            Debug.Log("You are lvl " + currentLevel + " now!");
        }
        if (levelIncreased)
        {
            Announcer.Announce("You are lvl " + currentLevel + " now!");
        }

        Registers.StatsRegister.SetStat(StatName.Level, currentLevel);
        Registers.StatsRegister.SetStat(StatName.CurrentXP, currentXp);
        Registers.StatsRegister.SetStat(StatName.FreeStatPoints, freeStatPoints);
    }
}