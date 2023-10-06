using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class LocationMap : Singleton<LocationMap>
{
    public static bool IsTileSpaceCreated => instance._mapRefference.IsCreated;

    public static TileTemplate[] TemplateMap { get => instance._templateMap; }
    public static BlobAssetReference<TileBlobAsset> MapRefference { get => instance._mapRefference; }

    private TileTemplate[] _templateMap;
    private BlobAssetReference<TileBlobAsset> _mapRefference;

    public static readonly int2[] NeiborsOffsetsArray8 = new int2[]
    {
            new int2(0,1),
            new int2(-1,0),
            new int2(1,0),
            new int2(0,-1),
            new int2(-1,1),
            new int2(1,1),
            new int2(-1,-1),
            new int2(1,-1),
    };

    public static readonly int2[] NeiborsOffsetsArray4 = new int2[]
    {
        new int2(0,1),
        new int2(-1,0),
        new int2(1,0),
        new int2(0,-1),
    };

    public static List<Entity>[] TileDropContainers;
    public static List<Entity>[] TileGroundCoverContainers;
    private static BlobBuilder blobBuilder;

    public static void DisposeMap()
    {
        if (IsTileSpaceCreated) instance._mapRefference.Dispose();
    }

    public static void SetTileData(TileData tileData)
    {
        MapRefference.Value.blobArray[tileData.index] = tileData;
    }

    public static TileData GetTileData(int index)
    {
        return instance._mapRefference.Value.blobArray[index];
    }

    public static void Clear()
    {
        WorldDisposer.DisposeWorld();
        DisposeMap();
        ClearContainers();
        ClearAuthorings();
        ClearTemplates();
        BuildBlob();

        void BuildBlob()
        {
            blobBuilder = new BlobBuilder(Allocator.Temp);
            ref TileBlobAsset currentMapAsset = ref blobBuilder.ConstructRoot<TileBlobAsset>();

            BlobBuilderArray<TileData> blobBuilderArray = blobBuilder.Allocate(ref currentMapAsset.blobArray, 4096);

            instance._mapRefference = blobBuilder.CreateBlobAssetReference<TileBlobAsset>(Allocator.Persistent);
            blobBuilder.Dispose();

            for (int i = 0; i < 4096; i++)
            {
                instance._mapRefference.Value.blobArray[i] = new TileData()
                {
                    index = i,
                    visible = false,
                    maped = false,
                    lightLevel = 0,
                    transitionId = -1,
                };

                TileDropContainers[i] = new List<Entity>();
                TileGroundCoverContainers[i] = new List<Entity>();
            }
        }

        void ClearAuthorings()
        {
            foreach (var item in FindObjectsOfType<EntityAuthoring>())
            {
                Pooler.Put(item.GetComponent<PoolableItem>());
            }
        }

        void ClearContainers()
        {
            TileDropContainers = new List<Entity>[4096];
            TileGroundCoverContainers = new List<Entity>[4096];
        }

        void ClearTemplates()
        {
            instance._templateMap = new TileTemplate[4096];

            for (int i = 0; i < 4096; i++)
            {
                instance._templateMap[i] = new TileTemplate(i);
            }
        }
    }
}

public struct TileBlobAsset
{
    public BlobArray<TileData> blobArray;
}