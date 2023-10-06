using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class TiledStructureImporter : MonoBehaviour
{
    public static StructureMap LoadStructure(StructureName structureName, Dictionary<int, SimpleObjectName> ObjectsNamesByTileMapId)
    {
        var path = Application.dataPath + "/Resources/TiledRooms/" + structureName + ".tmj";
        if (File.Exists(path))
        {
            var jsonstructure = JsonUtility.FromJson<JsonStructure>(File.ReadAllText(path));

            var width = jsonstructure.width;
            var height = jsonstructure.height;
            var map = new TileTemplate[jsonstructure.width * jsonstructure.height];
            var spawnPoints = new int2[10];
           

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    var flatIndex = new int2(x, y).ToMapIndex(width, height);
                    var template = TileTemplate.Null;

                    foreach (var layer in jsonstructure.layers)
                    {

                        var tileId = layer.data[flatIndex] - jsonstructure.tilesets[0].firstgid;
                        if (tileId < 1)
                        {
                            continue;
                        }
                        if (tileId == 11111111)
                        {
                            template.isCoridorStart = true;
                            continue;
                        }
                        if (tileId == 22222222)
                        {
                            template.isCoridorBlock = true;
                            continue;
                        }


                        if (tileId == 00000010)
                        {
                            spawnPoints[0] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000011)
                        {
                            spawnPoints[1] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000012)
                        {
                            spawnPoints[2] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000013)
                        {
                            spawnPoints[3] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000014)
                        {
                            spawnPoints[4] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000015)
                        {
                            spawnPoints[5] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000016)
                        {
                            spawnPoints[6] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000017)
                        {
                            spawnPoints[7] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000018)
                        {
                            spawnPoints[8] = new int2(x, y);
                            continue;
                        }
                        if (tileId == 00000019)
                        {
                            spawnPoints[9] = new int2(x, y);
                            continue;
                        }

                        template.GenerateObject(SimpleObjectsDatabase.GetObjectData(ObjectsNamesByTileMapId[tileId], true) , false);

                    }
                    map[flatIndex] = template;


                }
            }

            var structureMap = new StructureMap(width, height, spawnPoints, map);
            return structureMap;
        }

        Debug.Log(structureName + " structure is missing");
        return null;
    }



    public static TiledTileSet LoadTileset()
    {
        return JsonUtility.FromJson<TiledTileSet>(File.ReadAllText(Application.dataPath + "/Resources/TiledRooms/KatabasisTileset.tsj"));
    }


    [System.Serializable]
    public class TiledTileSet
    {
        public int columns = 0;
        public Grid grid = new Grid()
        {
            height = 1,
            orientation = "orthogonal",
            width = 1,
        };
        public int margin = 0;
        public string name = "KatabasisTiles";
        public int spacing = 0;
        public int tilecount;
        public string tiledversion = "1.8.4";
        public int tileheight = 32;
        public Tile[] tiles;
        public int tilewidth = 32;
        public string type = "tileset";
        public string version = "1.8";

        [System.Serializable]
        public struct Tile
        {
            public long id;
            public string image;
            public int imageheight;
            public int imagewidth;
            public string type;



            public TileProperty[] properties;
            [System.Serializable]
            public struct TileProperty
            {
                public string name;
                public string type;
                public string value;
            }
        }
        [System.Serializable]
        public struct Grid
        {
            public int height;
            public string orientation;
            public int width;
        }


    }
    [System.Serializable]
    private struct JsonStructure
    {
        public int height;
        public int width;

        public Layer[] layers;
        public UsedTileset[] tilesets;
        [System.Serializable]
        public struct Layer
        {
            public int[] data;
        }
        [System.Serializable]
        public struct UsedTileset
        {
            public int firstgid;
        }
    }
}

