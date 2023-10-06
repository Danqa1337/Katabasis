using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomizedGodIconData
{
    public KeyValuePair<TopGodIconPart, Color> TopGodIconPart { get; private set; }
    public KeyValuePair<MiddleGodIconPart, Color> MiddleGodIconPart { get; private set; }
    public KeyValuePair<BottomGodIconPart, Color> BottomGodIconPart { get; private set; }

    public RandomizedGodIconData(KeyValuePair<TopGodIconPart, Color> topGodIconPart, KeyValuePair<MiddleGodIconPart, Color> middleGodIconPart, KeyValuePair<BottomGodIconPart, Color> bottomGodIconPart)
    {
        TopGodIconPart = topGodIconPart;
        MiddleGodIconPart = middleGodIconPart;
        BottomGodIconPart = bottomGodIconPart;
    }
}
