using Assets.Scripts;
using System.Collections;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace ConsoleCommands
{
    public abstract class ConsoleCommand
    {
        public abstract ConsoleCommandResult Execute(string[] arguments);

        protected bool TryParseToFloat(string parseString, out float result)
        {
            if (float.TryParse(parseString, out result))
            {
                return true;
            }
            return false;
        }
    }

    public struct ConsoleCommandResult
    {
        public readonly string Message;
        public readonly string Color;

        public struct Error
        {
            public static ConsoleCommandResult InvalidTile => new ConsoleCommandResult("Selected tile is not valid", "red");
            public static ConsoleCommandResult InvalidArguments => new ConsoleCommandResult("Invalid arguments", "red");
            public static ConsoleCommandResult WrongObjectType => new ConsoleCommandResult("Selected tile has no entities of needed objectType", "red");

            public static ConsoleCommandResult Message(string message)
            {
                return new ConsoleCommandResult(message, "red");
            }
        }

        public struct Success
        {
            public static ConsoleCommandResult Default => Message("Sucess");

            public static ConsoleCommandResult Message(string message)
            {
                return new ConsoleCommandResult(message, "green");
            }
        }

        public ConsoleCommandResult(string message, string collor)
        {
            Message = message;
            Color = collor;
        }
    }

    public class Spawn : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (Selector.SelectedTile != null)
                {
                    if (Selector.SelectedTile != Player.CurrentTile)
                    {
                        var input = arguments[0];
                        var simpleObjectName = SimpleObjectName.Null;
                        dynamic data;
                        if (input.DecodeCharSeparatedEnumsAndGetFirst(out simpleObjectName))
                        {
                            data = SimpleObjectsDatabase.GetObjectData(simpleObjectName);
                        }
                        else
                        {
                            var complexObjectName = ComplexObjectName.Null;
                            if (input.DecodeCharSeparatedEnumsAndGetFirst<ComplexObjectName>(out complexObjectName))
                            {
                                data = ComplexObjectsDatabase.GetObjectData(complexObjectName);
                            }
                            else
                            {
                                data = ObjectDataFactory.GetComplexObjectDataFromString(input);
                            }
                        }

                        if (data != null)
                        {
                            Spawner.Spawn(data, Selector.SelectedTile);
                            return ConsoleCommandResult.Success.Message("Spawned " + input);
                        }
                        else
                        {
                            return ConsoleCommandResult.Error.InvalidArguments;
                        }
                    }
                    else
                    {
                        return ConsoleCommandResult.Error.Message("You can not spawn objects on the tile player standing");
                    }
                }
                else
                {
                    return ConsoleCommandResult.Error.InvalidTile;
                }
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidArguments;
            }
        }
    }

    public class LevelUp : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (TryParseToFloat(arguments[0], out float levels))
            {
                for (int i = 0; i < levels; i++)
                {
                    PlayerXPHandler.AddXP(Registers.StatsRegister.XPToNextLevel);
                }
                return ConsoleCommandResult.Success.Message("Added " + levels + " xls ");
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidArguments;
            }
        }
    }

    public class Suicide : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            Player.PlayerEntity.AddComponentData(new KillCreatureComponent());
            ManualSystemUpdater.Update<KillCreaturesSystem>();
            return ConsoleCommandResult.Success.Message("Commited suicide");
        }
    }

    public class Tp : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (Selector.SelectedTile.valid)
            {
                Player.PlayerEntity.AddComponentData(new MoveComponent(Player.PlayerEntity.CurrentTile(), Selector.SelectedTile, MovemetType.SelfPropeled));
                ManualSystemUpdater.Update<MovementSystem>();
                return ConsoleCommandResult.Success.Message("Teleported to " + Selector.SelectedTile.ToString());
            }
            return ConsoleCommandResult.Error.InvalidArguments;
        }
    }

    public class Resurrect : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var entity = Selector.SelectedTile.DropLayer.FirstOrDefault(e => e.HasComponent<CreatureComponent>());
            if (entity != Entity.Null)
            {
                Surgeon.Resurrect(entity);
                TimeController.SpendTime(1);
                return ConsoleCommandResult.Success.Message(entity.GetName() + " is ressurrected");
            }
            else
            {
                return ConsoleCommandResult.Error.Message("Nothing to resurrect here");
            }
        }
    }

    public class ChangeTimeScale : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                float scale = 0;
                if (TryParseToFloat(arguments[0], out scale))
                {
                    LowLevelSettings.instance.timescale = scale;
                    return ConsoleCommandResult.Success.Message("Timescale changed to " + scale);
                }
            }
            return ConsoleCommandResult.Error.InvalidArguments;
        }
    }

    public class MapAllTiles : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            BlobAssetReference<TileBlobAsset> tiles = LocationMap.MapRefference;
            for (int i = 0; i < tiles.Value.blobArray.Length; i++)
            {
                var tile = tiles.Value.blobArray[i];
                tile.visible = true;
                tile.maped = true;
                tile.lightLevel = 0.8f;
                tile.Save();
            }
            ManualSystemUpdater.Update<FOVSystem>();
            return ConsoleCommandResult.Success.Message("All map visible");
        }
    }

    public class MakeAbyss : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (!Selector.SelectedTile.isAbyss)
            {
                var floor = Selector.SelectedTile.FloorLayer;
                Selector.SelectedTile.Remove(floor);

                floor.AddComponentData(new DestroyEntityTag());

                FloorBaker.ClearTile(floor.CurrentTile().position, ObjectType.Floor);
                FloorBaker.Apply();
                ManualSystemUpdater.Update<EntityDestructionSystem>();
                Spawner.Spawn(SimpleObjectName.AbyssSide, Selector.SelectedTile);

                return ConsoleCommandResult.Success.Message("Abyss created");
            }
            else
            {
                return ConsoleCommandResult.Success.Message("Selected tile is abyss allready");
            }
        }
    }

    public class DetachPart : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var bodyPartTag = BodyPartTag.Null;
            if (arguments[0].DecodeCharSeparatedEnumsAndGetFirst<BodyPartTag>(out bodyPartTag))
            {
                var entity = Selector.SelectedTile.SolidLayer;
                if (entity != Entity.Null)
                {
                    var anatomy = entity.GetComponentData<AnatomyComponent>();
                    var part = anatomy.GetBodyPart(bodyPartTag);
                    if (part != Entity.Null)
                    {
                        entity.AddBufferElement(new AnatomyChangeElement(Entity.Null, bodyPartTag, Entity.Null));
                        TimeController.SpendTime(1);

                        return ConsoleCommandResult.Success.Message(bodyPartTag + " detached");
                    }
                    else
                    {
                        return ConsoleCommandResult.Error.Message("There is no such part");
                    }
                }
                else
                {
                    return ConsoleCommandResult.Error.WrongObjectType;
                }
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidArguments;
            }
        }
    }

    public class RestorePart : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var bodyPartTag = BodyPartTag.Null;
            if (arguments[0].DecodeCharSeparatedEnumsAndGetFirst<BodyPartTag>(out bodyPartTag))
            {
                var entity = Selector.SelectedTile.SolidLayer;
                if (entity != Entity.Null)
                {
                    if (entity.HasComponent<AnatomyComponent>())
                    {
                        if (entity.GetBuffer<MissingBodypartBufferElement>().Any(t => t.tag == bodyPartTag))
                        {
                            Surgeon.RestorePart(Selector.SelectedTile.SolidLayer, bodyPartTag);
                            return ConsoleCommandResult.Success.Message(bodyPartTag + " restored");
                        }
                        else
                        {
                            return ConsoleCommandResult.Error.Message("This part is allready attached");
                        }
                    }
                    else
                    {
                        return ConsoleCommandResult.Error.Message("There is anatomy");
                    }
                }
                else
                {
                    return ConsoleCommandResult.Error.WrongObjectType;
                }
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidArguments;
            }
        }
    }

    public class FindStructure : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                var structureName = StructureName.Null;
                if (arguments[0].DecodeCharSeparatedEnumsAndGetFirst<StructureName>(out structureName))
                {
                    if (StructureBuilder.FindStructure(Selector.SelectedTile, structureName))
                    {
                        return ConsoleCommandResult.Success.Message("Found " + structureName);
                    }

                    return ConsoleCommandResult.Error.Message(structureName + " not found");
                }
            }
            return ConsoleCommandResult.Error.InvalidArguments;
        }
    }

    public class Build : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                var structureName = StructureName.Null;
                if (arguments[0].DecodeCharSeparatedEnumsAndGetFirst<StructureName>(out structureName))
                {
                    StructureBuilder.BuildStructure(Selector.SelectedTile, structureName);
                    return ConsoleCommandResult.Success.Message("Built " + arguments[0]);
                }

                return ConsoleCommandResult.Error.Message(structureName + " not found");
            }
            return ConsoleCommandResult.Error.InvalidArguments;
        }
    }

    public class AddEffect : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (Selector.SelectedTile.SolidLayer != Entity.Null)
            {
                if (arguments.Length > 0)
                {
                    var entity = Selector.SelectedTile.SolidLayer;
                    var effectType = arguments[0].DecodeCharSeparatedEnumsAndGetFirst<EffectName>();
                    var duration = 100;

                    entity.AddBufferElement(new EffectElement(effectType, duration, Player.PlayerEntity));
                    TimeController.SpendTime(10);
                    return ConsoleCommandResult.Success.Message("Added " + effectType + " on " + entity.GetName() + " for " + duration + " ticks");
                }

                return ConsoleCommandResult.Error.InvalidArguments;
            }
            return ConsoleCommandResult.Error.WrongObjectType;
        }
    }

    public class GoToLocation : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var locationId = -1f;
            if (TryParseToFloat(arguments[0], out locationId))
            {
                var location = Registers.GlobalMapRegister.GetLocation((int)locationId);
                if (location == null)
                {
                    return ConsoleCommandResult.Error.Message("There is no location with such id");
                }

                LocationEnterManager.MoveToLocation(location, null, true);

                UiManager.ShowUiCanvas(UIName.Console);
                return ConsoleCommandResult.Success.Message("Teleported to location " + locationId);
            }
            return ConsoleCommandResult.Error.InvalidArguments;
        }
    }

    public class MakeEnemy : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var creature = Selector.SelectedTile.SolidLayer;
            if (creature != Entity.Null)
            {
                if (creature.HasComponent<AIComponent>())
                {
                    var ai = creature.GetComponentData<AIComponent>();
                    ai.target = Entity.Null;
                    ai.targetSearchCooldown = 0;
                    Registers.SquadsRegister.SplitfromSquad(Player.PlayerEntity.GetComponentData<SquadMemberComponent>().squadIndex, creature);
                    creature.SetComponentData(ai);

                    return ConsoleCommandResult.Success.Message(creature.GetName() + " is no longer your teammate");
                }
                else
                {
                    return ConsoleCommandResult.Error.Message("There is no AI");
                }
            }
            else
            {
                return ConsoleCommandResult.Error.WrongObjectType;
            }
        }
    }

    public class MakeAlly : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var creature = Selector.SelectedTile.SolidLayer;
            if (creature != Entity.Null)
            {
                if (creature.HasComponent<AIComponent>())
                {
                    var ai = creature.GetComponentData<AIComponent>();
                    ai.target = Entity.Null;
                    ai.targetSearchCooldown = 0;
                    Registers.SquadsRegister.AddToPlayersSquad(creature);

                    creature.SetComponentData(ai);
                    PlayerSquadsScreen.UpdatePortraits();
                    return ConsoleCommandResult.Success.Message(creature.GetName() + " is now your teammate");
                }
                else
                {
                    return ConsoleCommandResult.Error.Message("There is no AI");
                }
            }
            else
            {
                return ConsoleCommandResult.Error.WrongObjectType;
            }
        }
    }

    public class SpawnPack : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (Selector.SelectedTile != null)
                {
                    if (Selector.SelectedTile != Player.CurrentTile)
                    {
                        var packName = arguments[0].DecodeCharSeparatedEnumsAndGetFirst<PackName>();
                        Spawner.SpawnPack(packName, Selector.SelectedTile);
                        return ConsoleCommandResult.Success.Message(packName + " spawned");
                    }
                    else
                    {
                        return ConsoleCommandResult.Error.Message("You can not spawn objects on the tile player standing");
                    }
                }
                else
                {
                    return ConsoleCommandResult.Error.InvalidTile;
                }
            }
            else
            {
                return ConsoleCommandResult.Error.WrongObjectType;
            }
        }
    }

    public class AddToSquad : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (Selector.SelectedTile.SolidLayer != Entity.Null && Selector.SelectedTile.SolidLayer.HasComponent<CreatureComponent>())
                {
                    var squadIndex = 0f;
                    TryParseToFloat(arguments[0], out squadIndex);
                    Registers.SquadsRegister.MoveToSquad((int)squadIndex, Selector.SelectedTile.SolidLayer);
                    return ConsoleCommandResult.Success.Message("Added to squad " + squadIndex);
                }
                else
                {
                    return ConsoleCommandResult.Error.Message("Select a creature first!");
                }
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidArguments;
            }
        }
    }

    public class AddXP : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (TryParseToFloat(arguments[0], out float xpCount))
            {
                PlayerXPHandler.AddXP((int)xpCount);

                return ConsoleCommandResult.Success.Message("Added " + xpCount + " xp ");
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidArguments;
            }
        }
    }

    public class GrantPerk : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (Selector.SelectedTile != TileData.Null && Selector.SelectedTile.SolidLayer != Entity.Null && Selector.SelectedTile.SolidLayer.HasBuffer<PerkElement>())
            {
                if (arguments[0].DecodeCharSeparatedEnumsAndGetFirst<PerkName>(out PerkName perkName))
                {
                    PerksTree.GrantPerk(perkName, Selector.SelectedTile.SolidLayer);
                    TimeController.SpendTime(10);
                    return ConsoleCommandResult.Success.Message("Granted perk" + perkName + " to " + Selector.SelectedTile.SolidLayer.GetName());
                }
                else
                {
                    return ConsoleCommandResult.Error.InvalidArguments;
                }
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidTile;
            }
        }
    }

    public class RevokePerk : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (Selector.SelectedTile != TileData.Null && Selector.SelectedTile.SolidLayer != Entity.Null && Selector.SelectedTile.SolidLayer.HasBuffer<PerkElement>())
            {
                if (arguments[0].DecodeCharSeparatedEnumsAndGetFirst<PerkName>(out PerkName perkName))
                {
                    PerksTree.RevokePerk(perkName, Selector.SelectedTile.SolidLayer);
                    TimeController.SpendTime(10);
                    return ConsoleCommandResult.Success.Message("Revoked perk" + perkName + " from " + Selector.SelectedTile.SolidLayer.GetName());
                }
                else
                {
                    return ConsoleCommandResult.Error.InvalidArguments;
                }
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidTile;
            }
        }
    }

    public abstract class GodCommand : ConsoleCommand
    {
        public Gods.God GetGod(string[] arguments, out ConsoleCommandResult result)
        {
            if (arguments.Length == 2)
            {
                var godIndex = -1;
                if (int.TryParse(arguments[0], out godIndex))
                {
                    if (Registers.GodsRegister.GodExists(godIndex))
                    {
                        result = ConsoleCommandResult.Success.Default;
                        return Registers.GodsRegister.GetGod(godIndex);
                    }
                    else
                    {
                        result = ConsoleCommandResult.Error.Message("God not found: " + godIndex);
                    }
                }
                else
                {
                    result = ConsoleCommandResult.Error.Message("Can not parse god index " + arguments[0]);
                }
            }
            else
            {
                result = ConsoleCommandResult.Error.InvalidArguments;
            }
            return null;
        }
    }

    public class SetGodRelations : GodCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var error = new ConsoleCommandResult();
            var god = GetGod(arguments, out error);
            if (god != null)
            {
                var value = -1;
                if (int.TryParse(arguments[1], out value))
                {
                    god.SetRelations(Mathf.Clamp(value, -100, 100));
                    return ConsoleCommandResult.Success.Message("Relations with " + god + " set to " + value);
                }
                else
                {
                    return ConsoleCommandResult.Error.InvalidArguments;
                }
            }
            else
            {
                return error;
            }
        }
    }

    public class SetGodAttention : GodCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            var error = new ConsoleCommandResult();
            var god = GetGod(arguments, out error);
            if (god != null)
            {
                var value = -1;
                if (int.TryParse(arguments[1], out value))
                {
                    god.SetAttention(Mathf.Clamp(value, 0, 100));
                    return ConsoleCommandResult.Success.Message("Attention of " + god + " set to " + value);
                }
                else
                {
                    return ConsoleCommandResult.Error.InvalidArguments;
                }
            }
            else
            {
                return error;
            }
        }
    }

    public class Destroy : ConsoleCommand
    {
        public override ConsoleCommandResult Execute(string[] arguments)
        {
            if (Selector.SelectedTile.HoveringLayer != Entity.Null)
            {
                return DestroyEntity(Selector.SelectedTile.HoveringLayer);
            }
            else if (Selector.SelectedTile.SolidLayer != Entity.Null)
            {
                return DestroyEntity(Selector.SelectedTile.SolidLayer);
            }
            else if (Selector.SelectedTile.DropLayer.Length > 0)
            {
                return DestroyEntity(Selector.SelectedTile.DropLayer[0]);
            }
            else
            {
                return ConsoleCommandResult.Error.InvalidTile;
            }
        }

        private ConsoleCommandResult DestroyEntity(Entity entity)
        {
            var name = DescriberUtility.GetName(entity);
            entity.AddComponentData(new DestroyEntityTag());
            ManualSystemUpdater.Update<EntityDestructionSystem>();
            return ConsoleCommandResult.Success.Message(name + " is destroyed");
        }
    }
}