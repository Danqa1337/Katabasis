using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class TimeController : Singleton<TimeController>
{
    private bool _playerIsAlive = true;
    private static bool _debugSystems => LowLevelSettings.instance.debugSystems;
    private int _ticksToUpdate;
    private bool _isWorking;
    private EntityQueryDesc _defaultQueryDesc;

    public static event Action OnTurnStart;

    public static event Action OnSpendTime;

    public static event Action OnTurnEnd;

    public static event Action OnTickStarted;

    public static bool IsWorking => instance._isWorking;

    private void OnEnable()
    {
        Player.OnPlayerDied += delegate { _playerIsAlive = false; };
        Controller.OnControllerActionInvoked += ListenInput;
    }

    private void OnDisable()
    {
        Player.OnPlayerDied -= delegate { _playerIsAlive = false; };
        Controller.OnControllerActionInvoked -= ListenInput;
    }

    private void ListenInput(InputContext inputContext)
    {
        if (inputContext.Action == ControllerActionName.WaitForOneTick)
        {
            SpendTime(1);
        }
        if (inputContext.Action == ControllerActionName.WaitForOneTurn)
        {
            SpendTime(10);
        }
    }

    private void Awake()
    {
        _defaultQueryDesc = new EntityQueryDesc
        {
            Any = new ComponentType[]
            {
                typeof(ImpulseComponent),
                typeof(ChangeEquipmentElement),
                typeof(ChangeInventoryElement),
                typeof(AnatomyChangeElement),
                typeof(MoveComponent),
                typeof(MoraleChangeElement),
                typeof(DurabilityChangeElement),
                typeof(KillCreatureComponent),
            }
        };
    }

    private async void Update()
    {
        await UpdateTicks();
    }

    public static async Task UpdateTicks()
    {
        if (instance._ticksToUpdate > 0 && !instance._isWorking)
        {
            instance._isWorking = true;
            await StartNewTurn();
        }
    }

    private static async Task StartNewTurn()
    {
        OnTurnStart?.Invoke();
        instance._ticksToUpdate--;

        StartTick();

        var cycleQuery = new EntityQuery();

        int delayedCyclesIterations = 0;
        do
        {
            delayedCyclesIterations++;
            DelayedActionsUpdaterOnCycle.Update();

            int cycleIterations = 0;
            do
            {
                if (_debugSystems) Debug.Log("cycle");
                await UpdateStructuralCycle();
                cycleIterations++;
                cycleQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(instance._defaultQueryDesc);
            }
            while (!cycleQuery.IsEmpty && cycleIterations < 100 && instance._playerIsAlive);

            if (cycleIterations >= 100)
            {
                var existingComponents = new List<string>();
                foreach (var item in instance._defaultQueryDesc.Any)
                {
                    var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(item);
                    if (!query.IsEmpty)
                    {
                        existingComponents.Add("\n" + item.ToString());
                    }
                }
                throw new Exception("Structural cycle is out of limit. This components are still exist : " + String.Concat(existingComponents));
            }
        }
        while (!DelayedActionsUpdaterOnCycle.Empty && delayedCyclesIterations < 100 && instance._playerIsAlive);

        if (delayedCyclesIterations >= 100)
        {
            throw new Exception("Delayed actions cycle is out of limit. There are still " + DelayedActionsUpdaterOnCycle.GetActions().Count + " actions now");
        }

        EndTick();

        if (instance._ticksToUpdate == 0)
        {
            await EndTurn();
        }
        else
        {
            await StartNewTurn();
        }
    }

    public static void SpendTime(int ticks)
    {
        int t = Math.Max(1, (int)(ticks * LowLevelSettings.instance.timescale));
        instance._ticksToUpdate += t;
        OnSpendTime?.Invoke();
    }

    private static void StartTick()
    {
        if (_debugSystems) Debug.Log("new tick started ");
        OnTickStarted?.Invoke();
        DelayedActionsUpdaterOnTick.Update();

        UpdateSystem<EffectCoooldownSystem>();
        UpdateSystem<EffectActivationSystem>();
        UpdateSystem<EffectRealisationSystem>();
        UpdateSystem<EffectRemovingSystem>();

        UpdateSystem<AiCooldownSystem>();

        UpdateSystem<AgressionSystem>();

        UpdateSystem<AiSystem>();

        UpdateSystem<FindTargetSystem>();

        UpdateSystem<GetEvaluationDataSystem>();

        UpdateSystem<EvaluateBehaviourSystem>();

        UpdateSystem<PathfindingSystem>();

        UpdateSystem<ExecuteBehaviourSystem>();

        UpdateSystem<AbilitySystem>();

        UpdateSystem<SwapSystem>();

        UpdateSystem<CloudSystem>();

        UpdateSystem<ExchangeSystem>();

        UpdateSystem<MechansimsSystem>();

        UpdateSystem<OrdersSystem>();

        UpdateSystem<FireSystem>();
    }

    private static async Task UpdateStructuralCycle()
    {
        UpdateSystem<AnatomySystem>();

        UpdateSystem<EquipmentSystem>();

        UpdateSystem<InventorySystem>();

        UpdateSystem<PhysicsSystem>();

        UpdateSystem<MovementSystem>();

        UpdateSystem<CollisionSystem>();

        UpdateSystem<DurabilitySystem>();

        UpdateSystem<ObjectBreakSystem>();

        UpdateSystem<SacrificeSystem>();

        UpdateSystem<MoraleSystem>();

        UpdateSystem<HealthSystem>();

        UpdateSystem<KillCreaturesSystem>();

        UpdateSystem<SpillLiquidSystem>();

        UpdateSystem<ExplosionSystem>();

        UpdateSystem<PressurePlatesSystem>();

        UpdateSystem<SpawnSystem>();

        await World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AnimationSystem>().MinorUpdate();

        UpdateSystem<AbyssSystem>();

        UpdateSystem<SoundSystem>();
    }

    private static void EndTick()
    {
        UpdateSystem<EntityDestructionSystem>();
    }

    private static async Task EndTurn()
    {
        Debug.Log("Ending turn");

        UpdateSystem<FOVSystem>();

        UpdateSystem<TempObjectSystem>();

        UpdateSystem<OnTurnEndActionSystem>();

        UpdateSystem<ObjectVisibilitySystem>();

        UpdateSystem<OverHeadAnimationSystem>();

        await World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AnimationSystem>().MajorUpdate();

        instance._isWorking = false;
        OnTurnEnd?.Invoke();
    }

    public static void UpdateSystem<T>() where T : ComponentSystemBase
    {
        if (instance._playerIsAlive)
        {
            var system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<T>();
            system.Update();

            if (_debugSystems)
            {
                Debug.Log(system.ToString() + " updated ");
            }
        }
    }
}