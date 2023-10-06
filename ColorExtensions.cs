using System;
using UnityEngine;

public static class ColorExtensions
{
    public static Color RandomShift(this Color color, float value)
    {
        var newColor = color;
        newColor.r = Math.Clamp(newColor.r + UnityEngine.Random.Range(-value, value), 0, 1);
        newColor.g = Math.Clamp(newColor.g + UnityEngine.Random.Range(-value, value), 0, 1);
        newColor.b = Math.Clamp(newColor.b + UnityEngine.Random.Range(-value, value), 0, 1);
        return newColor;
    }
}
