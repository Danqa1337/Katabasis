using Unity.Collections;
using Unity.Mathematics;

public static class TileExtensions
{
    public static float GetDistance(this TileData start, TileData target)
    {
        return (target - start).Magnitude();
    }

    public static float GetSqrDistance(this TileData start, TileData target)
    {
        return (target - start).SqrMagnitude();
    }

    public static TileData ToTileData(this float2 vector)
    {
        return vector.ToMapIndex().ToTileData();
    }

    public static TileData ToTileData(this float2 vector, NativeArray<TileData> array)
    {
        return vector.ToMapIndex().ToTileData(array);
    }

    public static TileData ToTileData(this int2 vector)
    {
        return vector.ToMapIndex().ToTileData();
    }

    public static TileData ToTileData(this int2 vector, NativeArray<TileData> array)
    {
        return vector.ToMapIndex().ToTileData(array);
    }

    public static TileData ToTileData(this int index)
    {
        if (index != -1) return LocationMap.MapRefference.Value.blobArray[index];
        else return TileData.Null;
    }

    public static TileData ToTileData(this int index, NativeArray<TileData> array)
    {
        if (index != -1) return array[index];
        else return TileData.Null;
    }
}