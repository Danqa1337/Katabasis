//using System.Collections.Generic;
//using System.Linq;
//using Unity.Entities;
//using Unity.Mathematics;
//using UnityEngine;
//using Locations;

//public class ArenaDungeonPreset : GenerationPresetOld
//{
//    public override void Generate(Transition transition)
//    {
//        placeArenaFromObject();
//        GenerateAbyssSides();
//        ApplyTiles();

//        SpawnPlayer(transition);
//       // SpawnTestItems();
//        //SpawnDummy();

//       // SpawnArmory();

//        // GenerateTraps();

//        //Spawner.Spawn(ItemName.Cat, new int2(20, 15).ToTileData());
//        //Spawner.Spawn(ItemName.Rat, new int2(22, 15));
//        //  Spawner.SpawnPack(PackName.Merchants1, new int2(20, 20));

//        //SpawnAllies();
//        //SpawnMoney();
//        //SpawnHugeCrowd();
//        // SpwnTrader();
//        //SpawnTwoArmiesRangers();
//        //spawnFighters();
//        //spawnDummy();
//        //SpawnTwoArmies();
//        // SpawnOneArmy();

//        //mapObject.Save();
//        //GiveEquipmentToThePlayer();
//    }

//    private void SpawnMoney()
//    {
//        var tile = new int2(19, 17).ToTileData();
//        for (int i = 0; i < 10; i++)
//        {
//            tile.Spawn(ItemName.MediumGoldPiece);
//        }
//    }

//    private void SpwnTrader()
//    {
//        new int2(15, 15).ToTileData().Spawn(ItemName.HumanTrader);
//    }

//    private void spawnFighters()
//    {
//        var figter1 = Spawner.Spawn(ObjectDataFactory.GetRandomCreature(10, Biome.Dungeon, withRandomEquip: true, itemName: ItemName.Human), new int2(13, 13).ToTileData());
//        figter1.GetBuffer<EnemyTagBufferElement>().Add(new EnemyTagBufferElement(Tag.Human));

//        var figter2 = Spawner.Spawn(ObjectDataFactory.GetRandomCreature(10, Biome.Dungeon, withRandomEquip: true, itemName: ItemName.Human), new int2(15, 15).ToTileData());
//        figter2.GetBuffer<EnemyTagBufferElement>().Add(new EnemyTagBufferElement(Tag.Human));
//    }

//    protected override void GenerateTraps()
//    {
//        base.GenerateTraps();
//    }

//    private void SpawnAllies()
//    {
//        if (SquadsRegister.GetPlayersSquad().members.Count == 0)
//        {
//            var tiles = Player.playerEntity.CurrentTile().GetNeibors(true);
//            for (int i = 0; i < 1; i++)
//            {
//                var ally = tiles.RandomItem().Spawn(ItemName.Human);
//                SquadsRegister.MoveToSquad(1, ally);
//            }
//        }
//    }

//    private void SpawnTwoArmies()
//    {
//        var index1 = SquadsRegister.RegisterNewSquad();
//        var index2 = SquadsRegister.RegisterNewSquad();

//        foreach (var item in BaseMethodsClass.GetTilesInRadius(new int2(8, 8).ToTileData(), 4))
//        {
//            if (item.SolidLayer == Entity.Null)
//            {
//                var creature = item.Spawn(ObjectDataFactory.GetRandomCreature(10, itemName: ItemName.Human, withRandomEquip: true));
//                SquadsRegister.MoveToSquad(index1, creature);
//            }
//        }
//        foreach (var item in BaseMethodsClass.GetTilesInRadius(new int2(28, 8).ToTileData(), 4))
//        {
//            if (item.SolidLayer == Entity.Null)
//            {
//                var creature = item.Spawn(ObjectDataFactory.GetRandomCreature(10, itemName: ItemName.Human, withRandomEquip: true));
//                SquadsRegister.MoveToSquad(index2, creature);
//            }
//        }
//        SquadsRegister.AddEnemyIndex(index1, index2);
//        SquadsRegister.AddEnemyIndex(index2, index1);
//    }

//    private void SpawnOneArmy()
//    {
//        var index1 = SquadsRegister.RegisterNewSquad();

//        foreach (var item in BaseMethodsClass.GetTilesInRadius(new int2(8, 8).ToTileData(), 4))
//        {
//            if (item.SolidLayer == Entity.Null)
//            {
//                var creature = item.Spawn(ObjectDataFactory.GetRandomCreature(10, itemName: ItemName.Human, withRandomEquip: true));
//                SquadsRegister.MoveToSquad(index1, creature);
//            }
//        }
//    }

//    private void SpawnHugeCrowd()
//    {
//        foreach (var item in BaseMethodsClass.GetAllMapTiles())
//        {
//            if (item.SolidLayer == Entity.Null && Chance(50))
//            {
//                item.Spawn(ObjectDataFactory.GetRandomCreature(10, itemName: ItemName.Human, withRandomEquip: true));
//            }
//        }
//    }

//    private void SpawnTwoArmiesRangers()
//    {
//        var index1 = SquadsRegister.RegisterNewSquad();
//        var index2 = SquadsRegister.RegisterNewSquad();

//        foreach (var item in BaseMethodsClass.GetTilesInRadius(new int2(8, 8).ToTileData(), 2))
//        {
//            if (item.SolidLayer == Entity.Null)
//            {
//                var creature = item.Spawn(ItemName.HumanRanger);
//                SquadsRegister.MoveToSquad(index1, creature);
//            }
//        }
//        foreach (var item in BaseMethodsClass.GetTilesInRadius(new int2(28, 8).ToTileData(), 2))
//        {
//            if (item.SolidLayer == Entity.Null)
//            {
//                var creature = item.Spawn(ItemName.HumanRanger);
//                SquadsRegister.MoveToSquad(index2, creature);
//            }
//        }
//        SquadsRegister.AddEnemyIndex(index1, index2);
//        SquadsRegister.AddEnemyIndex(index2, index1);
//    }

//    public void SpawnDummy()
//    {
//        Spawner.Spawn(ItemName.Dummy, new int2(18, 18).ToTileData());
//    }

//    private void SpawnTestItems()
//    {
//        for (int i = 0; i < 10; i++)
//        {
//            Spawner.Spawn(ItemName.Grenade, new int2(17, 17).ToTileData());
//        }
//        Spawner.Spawn(ItemName.HumanRightArm, new int2(19, 17).ToTileData());
//    }

//    private void GiveEquipmentToThePlayer()
//    {
//        //for (int i = 0; i < 10; i++)
//        //{
//        //     Player.i.currentTile.GenerateObject("Arrow",false,false);
//        //}
//    }

//    private void spawnDummy()
//    {
//        //for (int i = 0; i < 50; i++)
//        //{
//        //    GetFreeTile().GenerateObject("OldCat");
//        //}
//    }

//    public void SpawnArmory()
//    {
//        StartTest();
//        var tiles = GetTilesInRectangleList(new int2(10, 2), new int2(20, 8));
//        var items = new List<SpawnData>();
//        foreach (var item in ItemDataBase.SpawnDataByName.Values.ToList())
//        {
//            if (item.objectData.dynamicData.objectType == ObjectType.Drop && item.objectData.staticData.bodyPartTag == BodyPartTag.Null)
//            {
//                items.Add(item);
//            }
//        }
//        for (int i = 0; i < items.Count; i++)
//        {
//            Spawner.Spawn(items[i].objectData, tiles[i]);
//        }
//        EndTest("Spawning armory took ");
//    }

//    public override void SpawnPlayer(Transition transition)
//    {
//        StartTest();
//        Spawner.SpawnPlayerSquad(new int2(10, 10).ToTileData());
//        EndTest("Spawning player took ");
//    }

//    public void placeArenaFromObject()
//    {
//        StartTest();
//        TileData leftCorner = new int2(0, 0).ToTileData();

//        StructureBuilder.BuildStructure(leftCorner, StructureDatabase.GetRoomData(StructureName.TestArena).structure);
//        EndTest("Building Arena took ");
//    }
//}