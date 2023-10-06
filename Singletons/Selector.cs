using System;
using System.IO;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class Selector : Singleton<Selector>
{
    private int _selectedTileId;
    private int _lastSelectedTileId;

    private PathFinderPath _path;

    public static event Action<TileData> OnSelectionChanged;

    public static event Action<PathFinderPath> OnPathChanged;

    public static TileData SelectedTile => instance._selectedTileId.ToTileData();
    public static TileData LastSelectedTile => instance._lastSelectedTileId.ToTileData();
    public static PathFinderPath Path => instance._path;

    private void OnEnable()
    {
        TimeController.OnTurnEnd += UpdateSelection;
    }

    private void OnDisable()
    {
        TimeController.OnTurnEnd -= UpdateSelection;
    }

    private void Update()
    {
        if (Controller.SelectionEnabled && LocationMap.IsTileSpaceCreated)
        {
            TileData tile = new float2
                (MainCameraHandler.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x + 0.5f,
                    MainCameraHandler.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()).y + 0.5f).ToTileData();
            SelectNewTile(tile);
        }
    }

    private void SelectNewTile(TileData tile)
    {
        if (tile.valid && SelectionMask.IsTileInsideMask(tile))
        {
            _lastSelectedTileId = _selectedTileId;
            _selectedTileId = tile.index;

            if (_selectedTileId != _lastSelectedTileId)
            {
                UpdateSelection();
            }
        }
        else
        {
            _selectedTileId = -1;
            OnSelectionChanged?.Invoke(TileData.Null);
        }
    }

    private void UpdateSelection()
    {
        OnSelectionChanged?.Invoke(_selectedTileId.ToTileData());
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (Player.PlayerEntity != Entity.Null && !SelectionMask.HasMask && SelectedTile.isWalkable(Player.PlayerEntity) && SelectedTile != Player.CurrentTile)
        {
            instance._path = Pathfinder.FindPath(Player.CurrentTile, SelectedTile, new WalkabilityDataComponent(Player.PlayerEntity), 1000, Unity.Collections.Allocator.Persistent);
        }
        else
        {
            _path = PathFinderPath.Null;
        }
        OnPathChanged?.Invoke(Path);
    }
}