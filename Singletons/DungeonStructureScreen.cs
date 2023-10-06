using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonStructureScreen : MonoBehaviour
{
    [SerializeField] private int verticalSpacing;
    [SerializeField] private int horizontalSpacing;
    public Color TransitionColor;
    public float transitionWidth;
    public List<GlobalMapTile> icons;
    public RectTransform holder;
    private float maxY = 0;
    public Sprite transitionSprite;

    public bool drawHidenLocations;

    public GlobalMapTile[,] mapTiles;

    private void OnEnable()
    {
        UiManager.OnShowCanvas += DoOnCanvasShow;
    }

    private void OnDisable()
    {
        UiManager.OnShowCanvas -= DoOnCanvasShow;
    }

    private void Awake()
    {
        mapTiles = new GlobalMapTile[10, 10];
        var tiles1D = GetComponentsInChildren<GlobalMapTile>();
        for (int i = 0; i < 100; i++)
        {
            var position = i.ToMapPosition(10, 10);
            mapTiles[position.x, position.y] = tiles1D[i];
        }
    }

    public void ReDraw()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                mapTiles[x, y].DrawRock();
            }
        }
        foreach (var location in Registers.GlobalMapRegister.Locations)
        {
            var X = 4;
            var Y = location.Level;

            if (location.GenerationPreset == GenerationPresetName.Pit)
            {
                X = 5;
            }
            if (location.GenerationPreset == GenerationPresetName.Arena)
            {
                X = 3;
                Y = 0;
            }

            var tile = GetMapTile(X, Y);
            if (!drawHidenLocations)
            {
                if (!location.IsGenerated)
                {
                    foreach (var transition in location.TransitionsIDs)
                    {
                        var location1 = Registers.GlobalMapRegister.GetLocation(Registers.GlobalMapRegister.GetTransition(transition).exitLoctionId);
                        var location2 = Registers.GlobalMapRegister.GetLocation(Registers.GlobalMapRegister.GetTransition(transition).entranceLocationId);
                        if (location1.IsGenerated || location2.IsGenerated)
                        {
                            tile.DrawUnknown();
                            break;
                        }
                        else
                        {
                            tile.DrawRock();
                        }
                    }
                }
                else
                {
                    tile.DrawLocation(location);
                }
            }
            else
            {
                tile.DrawLocation(location);
            }
        }
    }

    private GlobalMapTile GetMapTile(int x, int y)
    {
        return mapTiles[x, y];
    }

    private void Connect(GlobalMapTile locationA, GlobalMapTile locationB)
    {
        Vector2 posA = locationA.GetComponent<RectTransform>().anchoredPosition;
        Vector2 posB = locationB.GetComponent<RectTransform>().anchoredPosition;

        GameObject transition = new GameObject("Transition", typeof(Image));
        var canvas = transition.AddComponent<Canvas>();
        canvas.sortingLayerID = -1;
        transition.transform.SetParent(holder, true);
        var image = transition.GetComponent<Image>();
        image.color = Color.white;
        image.sprite = transitionSprite;
        image.type = Image.Type.Tiled;
        image.pixelsPerUnitMultiplier = 32;
        RectTransform rectTransform = transition.GetComponent<RectTransform>();
        Vector2 dir = (posB - posA).normalized;
        float distance = Vector2.Distance(posA, posB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, transitionWidth);
        rectTransform.anchoredPosition = posA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, CodeMonkey.Utils.UtilsClass.GetAngleFromVectorFloat(dir));
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        GlobalMapGenerator.instance.Generate();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        foreach (var item in holder.transform.GetComponentsInChildren<PoolableItem>())
        {
            Pooler.Put(item);
        }
    }

    private void DoOnCanvasShow(UIName uIName)
    {
        if (uIName == UIName.Map)
        {
            ReDraw();
        }
    }
}