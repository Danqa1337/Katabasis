using System;
using System.Collections.Generic;
using Unity.Entities;

public static class SeamlessTextureAligner
{
    private static Dictionary<string, int> codeDictionary;

    private static void FillDictionary()
    {
        codeDictionary = new Dictionary<string, int>();

        string[] codes = new string[]
        {
            "00001010",
            "00011000",
            "00011010",
            "00011000A",
            "00011000B",
            "00010010",
            "-1",
            "00000010",
            "01000010",
            "00001011",
            "01011110",
            "00011011",
            "00011111",
            "01011111",
            "00011111A",
            "01010110",
            "01001010",
            "01111010",
            "11010000",
            "01101010",
            "11111000",
            "11111011",
            "11111111",
            "11010110",
            "0100010A",
            "01001011",
            "00011110",
            "01011011",
            "00010110",
            "01101011",
            "11111111A",
            "11010110A",
            "0100010B",
            "01101011A",
            "11010110B",
            "01101000",
            "11011011",
            "01111111",
            "11111110",
            "11010010",
            "01001000",
            "01111011",
            "11011111",
            "00011111B",
            "01111110",
            "11111010",
            "11011010",
            "01010010",
            "-1B",
            "01101011B",
            "11111111B",
            "11111111C",
            "11011110",
            "01011010",
            "01010000",
            "01000000",
            "00001000",
            "01111000",
            "11111000A",
            "11111000B",
            "11011000",
            "01011000",
            "00010000",
            "00000000",
        };
        for (int i = 0; i < codes.Length; i++)
        {
            if (codeDictionary.ContainsKey(codes[i])) throw new KeyNotFoundException(i.ToString() + ", " + codes[i].ToString());
            codeDictionary.Add(codes[i], i);
        }
    }

    public static int GetSpriteNum(Entity entity)
    {
        var currentTile = entity.CurrentTile();
        FillDictionary();
        int[] code = new int[] { 1, 1, 1, 1, 1, 1, 1, 1 };
        TileData[] neibors = currentTile.GetNeibors(true);
        if (TexturesAreNOTEqual(neibors[4])) code[0] = 0;
        if (TexturesAreNOTEqual(neibors[5])) code[2] = 0;
        if (TexturesAreNOTEqual(neibors[6])) code[5] = 0;
        if (TexturesAreNOTEqual(neibors[7])) code[7] = 0;

        if (TexturesAreNOTEqual(neibors[0]))
        {
            code[0] = 0;
            code[1] = 0;
            code[2] = 0;
        }
        if (TexturesAreNOTEqual(neibors[1]))
        {
            code[0] = 0;
            code[3] = 0;
            code[5] = 0;
        }
        if (TexturesAreNOTEqual(neibors[2]))
        {
            code[2] = 0;
            code[4] = 0;
            code[7] = 0;
        }
        if (TexturesAreNOTEqual(neibors[3]))
        {
            code[5] = 0;
            code[6] = 0;
            code[7] = 0;
        }
        string s = "";
        foreach (var item in code)
        {
            s += item;
        }

        if (!codeDictionary.ContainsKey(s)) throw new KeyNotFoundException(s);
        if (codeDictionary[s] >= 64) throw new KeyNotFoundException(codeDictionary[s].ToString());
        return codeDictionary[s];

        bool TexturesAreNOTEqual(TileData tile)
        {
            if (tile == TileData.Null) return true;
            var type = entity.GetComponentData<ObjectTypeComponent>().objectType;
            List<Entity> entities = new List<Entity>();
            switch (type)
            {
                case ObjectType.Solid:
                    entities.Add(tile.SolidLayer);
                    break;
                case ObjectType.Drop:
                    entities.AddRange(tile.DropLayer);
                    break;
                case ObjectType.Floor:
                    entities.Add(tile.FloorLayer);

                    break;
                case ObjectType.Liquid:
                    entities.Add(tile.LiquidLayer);
                    break;
                case ObjectType.GroundCover:
                    entities.AddRange(tile.GroundCoverLayer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var item in entities)
            {
                if (item != Entity.Null && item.GetComponentData<SimpleObjectNameComponent>().simpleObjectName == entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName) return false;
            }
            return true;
        }
    }
}
