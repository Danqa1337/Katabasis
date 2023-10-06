using System;
using UnityEngine;

public class SelectionMask : Singleton<SelectionMask>
{
    [SerializeField] private Texture2D _selectionMask;
    public static bool HasMask => !instance._maskSettings.Equals(SelectionMaskSettings.Null);
    private SelectionMaskSettings _maskSettings = SelectionMaskSettings.Null;
    public struct SelectionMaskSettings
    {
        public readonly Func<TileData, bool> tileFunk;

        public SelectionMaskSettings(Func<TileData, bool> tileFunk)
        {
            this.tileFunk = tileFunk;
        }
        public static SelectionMaskSettings Null => new SelectionMaskSettings((t => true));
        
    }

    public static bool IsTileInsideMask(TileData tileData)
    {
        return instance._maskSettings.tileFunk(tileData);

    }
    public static void SetSelectionMask(SelectionMaskSettings tileSelectionMask, out bool anyAwaibleTileFound)
    {
        instance._maskSettings = tileSelectionMask;
        var colors = instance._selectionMask.GetPixels();
        anyAwaibleTileFound = false;
        for (int i = 0; i < colors.Length; i++)
        {
            if (tileSelectionMask.tileFunk(i.ToTileData()))
            {
                colors[i] = Color.clear;
                anyAwaibleTileFound = true;
            }
            else
            {
                colors[i] = Color.black;
            }
        }

        instance._selectionMask.SetPixels(colors);
        instance._selectionMask.Apply();
    }
    public static void ResetMask()
    {
        instance._selectionMask.Clear();
        instance._maskSettings = SelectionMaskSettings.Null;
    }



}