using UnityEngine;

public interface IHaveInventory
{

    public InventoryComponent inventory
    {
        get;
    }
    public abstract TileData currentTileData
    {
        get;
    }


}
public interface IAmInventory
{
    public Transform holder
    {
        get;
        set;
    }
    public IHaveInventory parent
    {
        get;
        set;
    }

}
