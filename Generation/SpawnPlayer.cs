using Unity.Mathematics;
using UnityEngine;

namespace GenerationModules
{
    public class SpawnPlayer : GenerationModule
    {
        [SerializeField] private int2 _fixedPosition;
        [SerializeField] private bool _useFixedPosition;

        public override void Generate(GenerationData generationData)
        {
            if (Application.isPlaying)
            {
                TileData tile = TileData.Null;
                if (_useFixedPosition)
                {
                    tile = _fixedPosition.ToTileData();
                }
                else
                {
                    if (generationData.DebugStairs)
                    {
                        tile = generationData.StaircaseDebugTile;
                    }
                    else
                    {
                        if (generationData.Transition != null)
                        {
                            if (generationData.Transition.entranceLocationId == Registers.GlobalMapRegister.CurrentLocationId)
                            {
                                tile = generationData.Transition.entrancePosition.ToTileData();
                            }
                            else
                            {
                                tile = generationData.Transition.exitPosition.ToTileData();
                            }
                        }
                        else
                        {
                            tile = GetFreeTile();
                        }
                    }
                    if (tile == TileData.Null)
                    {
                        throw new System.NullReferenceException("trying to spawn player on null tile");
                    }
                }

                Spawn(tile);

                UnityEngine.Debug.Log("spawned Player on " + tile.position);
            }
        }

        private void Spawn(TileData tile)
        {
            if (DataSaveLoader.SaveExists<PlayerSquadSaveData>())
            {
                var palyerSquad = DataSaveLoader.LoadData<PlayerSquadSaveData>();
                Spawner.SpawnPlayerSquad(palyerSquad, tile);
            }
            else
            {
                var playerName = ComplexObjectName.Human;
                if (UndestructableData.instance != null)
                {
                    playerName = UndestructableData.instance.playerRace;
                }
                var playerData = ComplexObjectsDatabase.GetObjectData(playerName);
                playerData.squadMemberComponent = new ComponentReferece<SquadMemberComponent>(new SquadMemberComponent(1));
                var playerSquad = new PlayerSquadSaveData(playerData, new System.Collections.Generic.List<ComplexObjectData>());
                Spawner.SpawnPlayerSquad(playerSquad, tile);
            }
        }
    }
}