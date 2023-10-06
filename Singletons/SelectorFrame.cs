using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SelectorFrame : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _frameFront;
    [SerializeField] private SpriteRenderer _frameBack;
    [SerializeField] private Texture2D _pathMask;

    private void OnEnable()
    {
        Selector.OnPathChanged += OnPathChanged;
        Selector.OnSelectionChanged += OnTileChanged;
    }

    private void OnDisable()
    {
        Selector.OnPathChanged -= OnPathChanged;
        Selector.OnSelectionChanged -= OnTileChanged;
    }

    private void Awake()
    {
        _pathMask.Clear();
        HideFrame();
    }

    private void OnTileChanged(TileData tileData)
    {
        if (tileData.valid)
        {
            transform.position = tileData.position.ToRealPosition();
            ShowFrame();
        }
        else
        {
            HideFrame();
        }

        var tileBelowLast = Selector.LastSelectedTile - new int2(0, 1);

        if (tileBelowLast.valid && tileBelowLast.SolidLayer != Entity.Null)
        {
            tileBelowLast.SolidLayer.GetComponentObject<RendererComponent>().OnTileAboveDeselected();
        }

        var tileBelowCurrent = tileData - new int2(0, 1);
        if (!tileData.visionBlocked && tileData.visible)
        {
            if (tileBelowCurrent.valid && tileBelowCurrent.SolidLayer != Entity.Null)
            {
                tileBelowCurrent.SolidLayer.GetComponentObject<RendererComponent>().OnTileAboveSelected();
            }
        }
    }

    private void OnPathChanged(PathFinderPath pathFinderPath)
    {
        _pathMask.Clear();
        if (pathFinderPath.IsCreated)
        {
            foreach (var item in pathFinderPath.Nodes)
            {
                _pathMask.SetPixel(item.x, item.y, Color.white);
            }
            _pathMask.Apply();
        }
    }

    private void HideFrame()
    {
        _frameFront.enabled = false;
        _frameBack.enabled = false;
    }

    private void ShowFrame()
    {
        _frameFront.enabled = true;
        _frameBack.enabled = true;
    }
}