using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using System.Collections;
using Abilities;

public class Player : Singleton<Player>
{
    private Entity _playerEntity;
    private bool _pathMovementStoped;
    public static TileData CurrentTile => PlayerEntity.CurrentTile();

    public static Action OnPlayerEquipChanged;
    public static Action OnPlayerDied;
    public static Action<Entity> OnPlayerDiedWithReason;
    public static Action OnPathTravelStart;
    public static Action OnPathTravelStop;

    public static Entity RightArm => PlayerEntity.GetComponentData<AnatomyComponent>().GetBodyPart((BodyPartTag.RightArm));
    public static Entity Head => PlayerEntity.GetComponentData<AnatomyComponent>().GetBodyPart((BodyPartTag.Head));
    public static Entity LeftArm => PlayerEntity.GetComponentData<AnatomyComponent>().GetBodyPart((BodyPartTag.LeftArm));
    public static Entity Body => PlayerEntity.GetComponentData<AnatomyComponent>().GetBodyPart((BodyPartTag.Body));
    public static EquipmentComponent EquipmentComponent => PlayerEntity.GetComponentData<EquipmentComponent>();
    public static AnatomyComponent AnatomyComponent => PlayerEntity.GetComponentData<AnatomyComponent>();
    public static InventoryComponent InventoryComponent => PlayerEntity.GetComponentData<InventoryComponent>();
    public static CreatureComponent CreatureComponent => PlayerEntity.GetComponentData<CreatureComponent>();
    public static Entity PlayerEntity { get => instance._playerEntity; }

    private void OnEnable()
    {
        Controller.OnControllerActionInvoked += ListenInput;
        RadialController.OnControllerActionInvoked += ListenInput;
        Spawner.OnPlayerSpawned += SetPlayerEntity;
    }

    private void OnDisable()
    {
        Controller.OnControllerActionInvoked -= ListenInput;
        RadialController.OnControllerActionInvoked -= ListenInput;
        Spawner.OnPlayerSpawned -= SetPlayerEntity;
    }

    private void SetPlayerEntity(Entity entity)
    {
        _playerEntity = entity;
        Registers.StatsRegister.SetStat(StatName.Agl, CreatureComponent.agl);
        Registers.StatsRegister.SetStat(StatName.Str, CreatureComponent.str);
    }

    private void ListenInput(InputContext inputContext)
    {
        StopPathTravel();
        switch (inputContext.Action)
        {
            case ControllerActionName.Give:
                Give(Selector.SelectedTile.SolidLayer, EquipmentComponent.itemInMainHand);
                break;

            case ControllerActionName.ToggleMechanism:
                ToggleMechanism();
                break;

            case ControllerActionName.Travel:

                if (!inputContext.IsPointerOverUi) StartPathTravel();
                break;

            case ControllerActionName.Eat:
                Eat();
                break;

            case ControllerActionName.Grab:
                PicUp();
                break;

            case ControllerActionName.Throw:
                Throw();
                break;

            case ControllerActionName.Jump:
                Jump();
                break;

            case ControllerActionName.LookAtTheFloor:
                LookAtTheGround();
                break;

            case ControllerActionName.Kick:
                KickSelected();
                break;

            case ControllerActionName.EnterStaircase:
                EnterStaircase();
                break;

            case ControllerActionName.OpenContainer:
                OpenContainer();
                break;

            case ControllerActionName.Sacrifice:
                Sacrifice();
                break;

            case ControllerActionName.ActionWithMainhand:
                if (!inputContext.IsPointerOverUi) ActionWithMainHand();
                break;

            case ControllerActionName.Pray:
                Pray();
                break;
        }
    }

    private void Pray()
    {
        Registers.GodsRegister.Pray();
    }

    public static List<Entity> GetAllItems()
    {
        var items = PlayerEntity.GetComponentData<InventoryComponent>().items;
        items.AddRange(PlayerEntity.GetComponentData<EquipmentComponent>().GetEquipmentNotNull());
        return items.ToList();
    }

    public static void Die(Entity responsibleEntity)
    {
        Debug.Log("you died. Responsible entity is " + responsibleEntity.GetName());
        Camera.main.transform.SetParent(null);
        CurrentTile.Drop(PlayerEntity);
        Controller.ChangeActionMap(ActionMap.Menuing);
        OnPlayerDied?.Invoke();
        OnPlayerDiedWithReason?.Invoke(responsibleEntity);
    }

    public static void MakeStep(TileData targeTile)
    {
        if (targeTile != PlayerEntity.CurrentTile())
        {
            if (targeTile.isWalkable(PlayerEntity))
            {
                PlayerEntity.AddComponentData(new AbilityComponent() { ability = AbilityName.MakeStep, targetTile = targeTile });
                UpdateSystem();
            }
            else
            {
                Debug.Log("Not walkable");
            }
        }
        else
        {
            Debug.Log("Is the same tile");
        }
    }

    public static void KickSelected()
    {
        var abilityData = AbilitiesDatabase.GetAbilityData(AbilityName.Kick);
        if (GnosisManager.CurrentGnosis >= abilityData.GnosisCost)
        {
            var abilityComponent = new AbilityComponent(AbilityName.Kick, Selector.SelectedTile);
            if (abilityData.Ability.TileFunc(PlayerEntity, abilityComponent))
            {
                PlayerEntity.AddComponentData(abilityComponent);
                UpdateSystem();
            }
        }
    }

    public static void PicUp()
    {
        if (PlayerEntity.CurrentTile().DropLayer.Where(d => !(d.HasComponent<AnatomyComponent>() && d.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull().Count > 1)).ToList().Count > 0)
        {
            var entity = CurrentTile.DropLayer.OrderBy(o => o.GetComponentData<PhysicsComponent>().baseAtackCost).ToList()[0];
            if (EquipmentComponent.itemInMainHand != Entity.Null)
            {
                PlayerEntity.AddComponentData(new AbilityComponent(AbilityName.PicUp, targetEntity: entity));
                UpdateSystem();
                ManualSystemUpdater.Update<InventorySystem>();
            }
            else
            {
                PlayersEquip.PlaceItem(entity, EquipTag.Weapon);
            }
        }
    }

    public static void LookAtTheGround()
    {
        UiManager.ShowUiCanvas(UIName.Inventory);
        UiManager.ShowUiCanvas(UIName.ItemsOnTheGround);
        ItemsOnTheGround.UpdateItems();
    }

    public static void OpenContainer()
    {
        if (Selector.SelectedTile.SolidLayer.HasComponent<ContainerComponent>())
        {
            UiManager.ShowUiCanvas(UIName.Inventory);
            UiManager.ShowUiCanvas(UIName.Container);
            ContainerOpener.OpenContainer(Selector.SelectedTile.SolidLayer);
        }
    }

    public static void Throw()
    {
        var item = EquipmentComponent.itemInMainHand;
        if (item == Entity.Null)
        {
            item = EquipmentComponent.itemInOffHand;
        }
        if (item != Entity.Null)
        {
            if (Selector.SelectedTile != CurrentTile)
            {
                if (Selector.SelectedTile.IsInRangeOfOne(CurrentTile) && Selector.SelectedTile.SolidLayer.HasComponent<AIComponent>())
                {
                    Give(Selector.SelectedTile.SolidLayer, item);
                }
                else
                if (!CurrentTile.GetNeibors(true).Contains(Selector.SelectedTile) || Selector.SelectedTile.SolidLayer == Entity.Null)
                {
                    PlayerEntity.AddComponentData(new AbilityComponent() { ability = AbilityName.Throw, targetEntity = item, targetTile = Selector.SelectedTile });

                    UpdateSystem();
                }
            }
        }
    }

    public static void Give(Entity creature, Entity item)
    {
        if (!creature.HasComponent<AliveTag>())
        {
            return;
        }

        if (creature.HasComponent<TraderComponent>() || creature.HasComponent<SlaverComponent>() && !creature.IsHostile(PlayerEntity))
        {
            var tradingItem = creature.GetComponentData<EquipmentComponent>().itemInMainHand;
            ExchangeSystem.AddOffer(new ExchangeOffer(PlayerEntity, creature, item, tradingItem));
        }
        else if (creature.IsPlayersSquadmate())
        {
            ExchangeSystem.AddOffer(new ExchangeOffer(PlayerEntity, creature, item, Entity.Null));
        }
        PlayerEntity.AddComponentData(new AbilityComponent() { ability = AbilityName.SpendTime10 });
        UpdateSystem();
    }

    private static void Sacrifice()
    {
        var targetTile = Selector.SelectedTile;
        if (targetTile.GetNeibors(true).Contains(CurrentTile) && targetTile.SolidLayer == Entity.Null)
        {
            targetTile.isAltar = true;
            targetTile.Save();
            Debug.Log("Altar placed on " + targetTile);
            TimeController.SpendTime(10);
            Spawner.Spawn(SimpleObjectName.Fire, CurrentTile);
        }
    }

    public static void ActionWithMainHand()
    {
        if (Selector.SelectedTile.IsInRangeOfOne(CurrentTile) && Selector.SelectedTile.SolidLayer.HasComponent<ContainerComponent>())
        {
            OpenContainer();
        }
        else
        {
            if (PlayerEntity.GetComponentData<EquipmentComponent>().itemInMainHand.HasComponent<RangedWeaponComponent>())
            {
                PlayerEntity.AddComponentData(new AbilityComponent(AbilityName.Shoot, Selector.SelectedTile));
                UpdateSystem();
            }
            else
            {
                var tilesInRange = EquipmentComponent.itemInMainHand.HasComponent<PolearmTag>() ? PlayerEntity.CurrentTile().GetTilesInRadius(2).ToList() : PlayerEntity.CurrentTile().GetNeibors(true).ToList();

                tilesInRange = tilesInRange.Where(t => t.visible).ToList();

                if (tilesInRange.Contains(Selector.SelectedTile))
                {
                    PlayerEntity.AddComponentData(new AbilityComponent(AbilityName.Attack, Selector.SelectedTile));
                }
            }

            UpdateSystem();
        }
    }

    public static void ToggleMechanism()
    {
        PlayerEntity.AddComponentData(new AbilityComponent(AbilityName.ToggleMechanism, Selector.SelectedTile));
        UpdateSystem();
    }

    public static void Jump()
    {
        var abilityData = AbilitiesDatabase.GetAbilityData(AbilityName.Jump);
        if (GnosisManager.CurrentGnosis >= abilityData.GnosisCost)
        {
            var abilityComponent = new AbilityComponent(AbilityName.Jump, Selector.SelectedTile);
            if (abilityData.Ability.TileFunc(PlayerEntity, abilityComponent))
            {
                PlayerEntity.AddComponentData(abilityComponent);
                UpdateSystem();
            }
        }
    }

    public static void Eat()
    {
        var currentTile = PlayerEntity.CurrentTile();
        var eatableItem = Entity.Null;

        if (currentTile.DropLayer.Any(IsEatableItem))
        {
            eatableItem = currentTile.DropLayer.First(IsEatableItem);
            Eat(eatableItem);
        }
        else if (currentTile.DropLayer.Any(i => i.HasComponent<EatableComponent>() && i.HasComponent<AnatomyComponent>()))
        {
            eatableItem = currentTile.DropLayer.First(i => i.HasComponent<EatableComponent>() && i.HasComponent<AnatomyComponent>());
            PlayerEntity.AddComponentData(new AbilityComponent(AbilityName.Butcher, targetEntity: eatableItem));
            UpdateSystem();
        }
    }

    public static void Eat(Entity entity)
    {
        PlayerEntity.AddComponentData(new AbilityComponent(AbilityName.Eat, targetEntity: entity));

        if (InventoryComponent.items.Contains(entity))
        {
            PlayerEntity.AddBufferElement(new ChangeInventoryElement(entity, false));
        }
        else if (EquipmentComponent.itemInMainHand == entity)
        {
            PlayerEntity.AddBufferElement(new ChangeEquipmentElement(Entity.Null, EquipTag.Weapon));
        }
        else if (EquipmentComponent.itemInOffHand == entity)
        {
            PlayerEntity.AddBufferElement(new ChangeEquipmentElement(Entity.Null, EquipTag.Shield));
        }
        UpdateSystem();
    }

    public static bool IsEatableItem(Entity entity)
    {
        return entity.HasComponent<EatableComponent>() && (!entity.HasComponent<DecayComponent>() || !entity.GetComponentData<DecayComponent>().isDecayed);
    }

    public static void StartPathTravel()
    {
        if (Selector.SelectedTile.maped && Selector.SelectedTile != CurrentTile)
        {
            if (Selector.SelectedTile.isWalkable(PlayerEntity) && Selector.Path.Length > 0)
            {
                instance.StartCoroutine(instance.MoveByPath(Selector.SelectedTile));
            }
        }
    }

    private IEnumerator MoveByPath(TileData targetTile)
    {
        OnPathTravelStart?.Invoke();
        var t = 0;
        _pathMovementStoped = false;
        while (targetTile != CurrentTile && t < 100 && !_pathMovementStoped)
        {
            t++;
            var path = Pathfinder.FindPath(CurrentTile, targetTile, new WalkabilityDataComponent(PlayerEntity), 1000);
            if (path != PathFinderPath.Null)
            {
                var nextTile = path.Nodes[0].ToTileData();
                path.Dispose();
                MakeStep(nextTile);

                do
                {
                    yield return new WaitForEndOfFrame();
                }
                while (TimeController.IsWorking);
            }
            else
            {
                break;
            }
        }
        OnPathTravelStop?.Invoke();
    }

    private void StopPathTravel()
    {
        _pathMovementStoped = true;
    }

    public static void EnterStaircase()
    {
        if (CurrentTile.transitionId != -1)
        {
            LocationEnterManager.EnterTransition(Registers.GlobalMapRegister.GetTransition(CurrentTile.transitionId));
        }
        else
        {
            Debug.Log("There is no staircase on " + CurrentTile.position);
        }
    }

    private static void UpdateSystem()
    {
        ManualSystemUpdater.Update<AbilitySystem>();
    }
}