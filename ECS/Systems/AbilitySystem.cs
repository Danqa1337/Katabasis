using Assets.Scripts;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public partial class AbilitySystem : MySystemBase
{
    private Entity self;
    protected AbilityComponent abilityComponent;
    protected AnatomyComponent anatomy;
    protected TileData selfCurrentTile;
    protected EquipmentComponent equipment;
    protected InventoryComponent inventory;
    protected PhysicsComponent physics;
    protected EntityCommandBuffer ecb;
    public bool debug => _debug;

    public new void AddToLastDebugMessage(string message)
    {
        base.AddToLastDebugMessage(message);
    }

    protected void UpdateValues(Entity entity)
    {
        self = entity;
        if (self.HasComponent<AbilityComponent>()) abilityComponent = self.GetComponentData<AbilityComponent>();
        anatomy = self.GetComponentData<AnatomyComponent>();
        selfCurrentTile = self.CurrentTile();
        equipment = self.GetComponentData<EquipmentComponent>();
        inventory = self.GetComponentData<InventoryComponent>();
        physics = self.GetComponentData<PhysicsComponent>();
    }

    protected override void OnUpdate()
    {
        _debug = LowLevelSettings.instance.debugAbilities;

        float startTime = UnityEngine.Time.realtimeSinceStartup;
        ecb = CreateEntityCommandBuffer();

        var entitiesCount = 0;
        Entities.ForEach((Entity entity, in AbilityComponent abilityComponent) =>
        {
            UpdateValues(entity);

            if (entity.HasComponent<AliveTag>())
            {
                if (_debug) NewDebugMessage(entity.GetName() + " uses aility " + abilityComponent.ability);
                entitiesCount++;
                var abilityData = AbilitiesDatabase.GetAbilityData(abilityComponent.ability);

                if (abilityData.Ability.TileFunc(entity, abilityComponent))
                {
                    abilityData.Ability.DoAction(entity, abilityComponent, ecb);
                }
                else
                {
                    AddToLastDebugMessage("..but conditions dont match");
                }
                if (entity.IsPlayer())
                {
                    GnosisManager.RemoveGnosis(abilityData.GnosisCost);
                }
                var cooldown = abilityData.BaseCooldown + abilityData.Ability.AdditionalCooldown(entity, abilityComponent);
                SpendTime(cooldown);
            }
            ecb.RemoveComponent<AbilityComponent>(entity);
        }).WithoutBurst().Run();

        WriteDebug();
        if (ecb.IsCreated)
        {
            UpdateECB();
        }
    }

    public void SpendTime(int ticks)
    {
        if (self.HasEffect(EffectName.Haste)) ticks = Mathf.Max(1, (int)(ticks / 2f));
        if (self.HasEffect(EffectName.Slowness)) ticks = Mathf.Max(1, (int)(ticks * 2f));

        if (self.IsPlayer())
        {
            if (ticks > 0)
            {
                Debug.Log("player is waiting for " + ticks + " ticks");
                TimeController.SpendTime(ticks);
            }
        }
        else
        {
            var ai = self.GetComponentData<AIComponent>();
            ai.abilityCooldown += ticks;
            ecb.SetComponent(self, ai);
        }
    }
}