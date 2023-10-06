using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct PathNode
{
    public int X;
    public int Y;
    public int2 position => new int2(X, Y);

    public float gCost;
    public float hCost;
    public float fCost;

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public int parentIndex;
    private int heapIndex;

    public PathNode(int index, int2 startPosition, int2 targetPosition)
    {
        var position = index.ToMapPosition();
        var hCost = Pathfinder.GetHCost(position, targetPosition);
        var dx1 = position.x - targetPosition.x;
        var dy1 = position.y - targetPosition.y;
        var dx2 = startPosition.x - targetPosition.x;
        var dy2 = startPosition.y - targetPosition.y;
        var cross = math.abs(dx1 * dy2 - dx2 * dy1);
        hCost += cross * 0.1f;

        this.X = position.x;
        this.Y = position.y;
        this.parentIndex = -1;
        this.heapIndex = index;
        this.gCost = float.MaxValue;
        this.fCost = float.MaxValue;
        this.hCost = hCost;
    }

    public int index
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(PathNode nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

    public static bool operator ==(PathNode A, PathNode B)
    {
        if (A.X == B.X && A.Y == B.Y) return true;
        return false;
    }

    public static bool operator !=(PathNode A, PathNode B)
    {
        if (A.X != B.X || A.Y != B.Y) return true;
        return false;
    }
}