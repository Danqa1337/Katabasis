using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public List<Transform> tiles;

    public Transform NewTile()
    {
        GameObject tile = new GameObject("Tile");
        tiles.Add(tile.transform);
        return tile.transform;
    }

}
