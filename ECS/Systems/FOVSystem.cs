using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[DisableAutoCreation]
public partial class FOVSystem : MySystemBase
{
    private bool _fovUpdateScheduled;

    protected override void OnUpdate()
    {
        if (_fovUpdateScheduled)
        {
            _fovUpdateScheduled = false;

            var colors = new NativeArray<Color>(128 * 128, Allocator.TempJob);

            var playerSquadPositions = Registers.SquadsRegister.GetSquadmates(SquadsRegister.PlayerSquadIndex).Select(e => e.CurrentTile().position).ToArray();
            var mapedTilesNum = new NativeReference<int>(Allocator.Temp);
            var newMapedTransitions = new NativeParallelHashMap<int, bool>(Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs.Count, Allocator.Temp);

            foreach (var id in Registers.GlobalMapRegister.CurrentLocation.TransitionsIDs)
            {
                newMapedTransitions.Add(id, false);
            }

            new SetupFovColorsJob()
            {
                map = LocationMap.MapRefference,
                colors = colors,
            }.Schedule().Complete();

            for (int i = 0; i < playerSquadPositions.Length; i++)
            {
                var handles = new JobHandle[8];
                SetColor(playerSquadPositions[i].ToMapIndex(), Color.clear, colors, 0);
                SetColor(playerSquadPositions[i].ToMapIndex(), Color.clear, colors, 1);

                for (int o = 0; o < 8; o++)
                {
                    var fovJob = new OcatgonalFovJobRestrictive()
                    {
                        player = playerSquadPositions[i],
                        map = LocationMap.MapRefference,
                        VisualRange = StatsCalculator.CalculateViewDistance(Player.PlayerEntity, Registers.GlobalMapRegister.CurrentLocation),
                        maxDarcnessA = LowLevelSettings.instance.maxDarknessA,
                        colors = colors,
                        octatntNum = o,
                        mapedTilesNum = mapedTilesNum,
                        newMapedTransitions = newMapedTransitions,
                    };
                    handles[o] = fovJob.Schedule();
                }

                foreach (var item in handles)
                {
                    item.Complete();
                }
            }

            if (mapedTilesNum.Value > 0)
            {
                GnosisManager.AddGnosis(mapedTilesNum.Value * 2);
            }
            foreach (var pair in newMapedTransitions)
            {
                if (pair.Value == true)
                {
                    StairsInfo.DrawNewIcon(Registers.GlobalMapRegister.GetTransition(pair.Key));
                }
            }
            LowLevelSettings.instance.FOV.SetPixels(colors.ToArray());
            LowLevelSettings.instance.FOV.Apply();
            colors.Dispose();
            mapedTilesNum.Dispose();
            newMapedTransitions.Dispose();
        }
    }

    public static void ScheduleFOWUpdate()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<FOVSystem>()._fovUpdateScheduled = true;
    }

    [GenerateTestsForBurstCompatibility]
    public static void SetColor(int index, Color color, NativeArray<Color> colors, int row)
    {
        var offset = new int2(0, row);

        var position = index.ToMapPosition() * 2;

        var index0 = (position + offset).ToMapIndex(128, 128);
        var index1 = (position + new int2(1, 0) + offset).ToMapIndex(128, 128);

        if (index0 != -1) colors[index0] = color;
        if (index1 != -1) colors[index1] = color;
    }

    [BurstCompile]
    private struct OcatgonalFovJobRestrictive : IJob
    {
        [NativeDisableContainerSafetyRestriction] public BlobAssetReference<TileBlobAsset> map;
        [ReadOnly] public int2 player;
        [ReadOnly] public int VisualRange;
        [ReadOnly] public float maxDarcnessA;
        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<Color> colors;
        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeReference<int> mapedTilesNum;
        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeParallelHashMap<int, bool> newMapedTransitions;

        [ReadOnly] public int octatntNum;
        public const int mapSizeX = 64;
        public const int mapSizeY = 64;

        public void Execute()
        {
            var octantProperties = GetOctantProperties(octatntNum);

            var shadows = new NativeList<Shadow>(Allocator.Temp);
            var sqrViewDistance = VisualRange * VisualRange;
            for (int y = 0; y < VisualRange; y++)
            {
                var rowLength = y + 1;
                for (int x = 0; x < rowLength; x++)
                {
                    var tile = GetTile(x, y, octantProperties);
                    if (tile == TileData.Null) continue;
                    if ((tile.position - player).SqrMagnitude() <= sqrViewDistance)
                    {
                        float oneTileAngle = 1f / rowLength;
                        float startAngle = oneTileAngle * x;
                        float endAngle = oneTileAngle * (x + 1f);
                        float midAngle = startAngle + oneTileAngle / 2f;

                        var visible = true;
                        var startAngleBlocked = false;
                        var midAngleBlocked = false;
                        var endAngleBlocked = false;

                        for (int s = 0; s < shadows.Length; s++)
                        {
                            var shadow = shadows[s];

                            if (!startAngleBlocked) startAngleBlocked = shadow.Covers(startAngle);
                            if (!midAngleBlocked) midAngleBlocked = shadow.Covers(midAngle);
                            if (!endAngleBlocked) endAngleBlocked = shadow.Covers(endAngle);

                            if (tile.visionBlocked)
                            {
                                if (startAngleBlocked && endAngleBlocked && midAngleBlocked)
                                {
                                    visible = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (midAngleBlocked)
                                {
                                    visible = false;
                                    break;
                                }
                                else if (startAngleBlocked && endAngleBlocked)
                                {
                                    visible = false;
                                    break;
                                }
                            }
                        }
                        if (visible)
                        {
                            MarkAsVisible(tile.index, 1);
                            if (tile.visionBlocked)
                            {
                                MarkAsVisible(tile.index, 2);
                            }
                            var indexBelow = (tile.position + new int2(0, -1)).ToMapIndex();
                            if (indexBelow != -1 && !map.Value.blobArray[indexBelow].visionBlocked)
                            {
                                MarkAsVisible(tile.index, 0);
                            }
                        }
                        if (tile.visionBlocked)
                        {
                            shadows.Add(new Shadow(startAngle, endAngle, y, tile.index));
                        }
                    }
                }
            }

            shadows.Dispose();
        }

        public TileData GetTile(int x, int y, OctantProperties octantProperties)
        {
            var index = new int2(player.x + x * octantProperties.xx + y * octantProperties.xy, player.y + x * octantProperties.yx + y * octantProperties.yy).ToMapIndex();
            if (index != -1)
            {
                return map.Value.blobArray[index];
            }
            return TileData.Null;
        }

        public void MarkAsVisible(int index, int row)
        {
            var tile = map.Value.blobArray[index];
            if (!tile.maped)
            {
                mapedTilesNum.Value++;
                if (tile.transitionId != -1)
                {
                    newMapedTransitions[tile.transitionId] = true;
                }
            }
            tile.maped = true;
            tile.visible = true;
            map.Value.blobArray[tile.index] = tile;
            var color = Color.clear;
            FOVSystem.SetColor(index, color, colors, row);
        }

        private OctantProperties GetOctantProperties(int octatntNum)
        {
            switch (octatntNum)
            {
                case 0:
                    return new OctantProperties(1, 0, 0, 1);

                case 1:
                    return new OctantProperties(0, 1, 1, 0);

                case 2:
                    return new OctantProperties(0, -1, 1, 0);

                case 3:
                    return new OctantProperties(-1, 0, 0, 1);

                case 4:
                    return new OctantProperties(-1, 0, 0, -1);

                case 5:
                    return new OctantProperties(0, -1, -1, 0);

                case 6:
                    return new OctantProperties(0, 1, -1, 0);

                case 7:
                    return new OctantProperties(1, 0, 0, -1);
            }
            return new OctantProperties();
        }

        public struct Shadow
        {
            public float startAngle;
            public float endAngle;
            public int row;
            public int index;

            public Shadow(float startAngle, float endAngle, int row, int index)
            {
                this.startAngle = startAngle;
                this.endAngle = endAngle;
                this.row = row;
                this.index = index;
            }

            public bool Covers(float angle)
            {
                return angle >= startAngle /*- 0.01f*/ && angle <= endAngle;// + 0.01f;
            }
        }

        public struct OctantProperties
        {
            public int xx { get; private set; }
            public int xy { get; private set; }
            public int yx { get; private set; }
            public int yy { get; private set; }

            public OctantProperties(int xx, int xy, int yx, int yy)
            {
                this.xx = xx;
                this.xy = xy;
                this.yx = yx;
                this.yy = yy;
            }
        }
    }

    [BurstCompile]
    private struct SetupFovColorsJob : IJob
    {
        public BlobAssetReference<TileBlobAsset> map;
        [WriteOnly] public NativeArray<Color> colors;

        public void Execute()
        {
            for (int i = 0; i < map.Value.blobArray.Length; i++)
            {
                var tile = map.Value.blobArray[i];
                tile.visible = false;
                map.Value.blobArray[i] = tile;
                var color = Color.white;
                FOVSystem.SetColor(tile.index, Color.white, colors, 0);
                FOVSystem.SetColor(tile.index, Color.white, colors, 1);
            }
            for (int i = 0; i < map.Value.blobArray.Length; i++)
            {
                var tile = map.Value.blobArray[i];
                if (tile.maped)
                {
                    var color = new Color(1, 1, 1, 0.9f);

                    FOVSystem.SetColor(i, color, colors, 1);

                    if (tile.visionBlocked)
                    {
                        FOVSystem.SetColor(i, color, colors, 2);
                        var indexBelow = (tile.position + new int2(0, -1)).ToMapIndex();
                        if (indexBelow != -1)
                        {
                            var tileBelow = map.Value.blobArray[indexBelow];
                            if (!tileBelow.visionBlocked)
                            {
                                FOVSystem.SetColor(i, color, colors, 0);
                            }
                        }
                    }
                    else
                    {
                        FOVSystem.SetColor(i, color, colors, 0);
                    }
                }
            }
        }
    }
}