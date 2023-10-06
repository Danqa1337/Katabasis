using Assets.Scripts;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public enum AnatomyChangeType
{
    Add,
    Remove,
}

[DisableAutoCreation]
public partial class AnatomySystem : MySystemBase
{
    public static event Action OnPlayerAnatomyChanged;

    protected override void OnUpdate()
    {
        var random = GetRandom();
        var ecb = CreateEntityCommandBuffer();
        _debug = LowLevelSettings.instance.debugAnatomy;
        Entities.ForEach((Entity entity,
            EntityAuthoring authoring,
            ref DynamicBuffer<AnatomyChangeElement> anatomyChangesBuffer,
            ref DynamicBuffer<MissingBodypartBufferElement> missingParts,
            ref AnatomyComponent anatomyComponent,
            in CurrentTileComponent currentTileComponent,
            in EquipmentComponent equipmentComponent) =>
        {
            foreach (var element in anatomyChangesBuffer)
            {
                var bodyPartTag = element.BodyPartTag;
                if (bodyPartTag == BodyPartTag.Any)
                {
                    bodyPartTag = element.part.GetComponentData<BodyPartComponent>().bodyPartTag;
                }

                var field = anatomyComponent.GetType().GetField(bodyPartTag.ToString());

                if (field == null)
                {
                    throw new Exception(bodyPartTag + " is not added as field");
                }

                var currentPart = (Entity)field.GetValue(anatomyComponent);

                if (element.part == entity)
                {
                    throw new Exception("Trying to detach body");
                }
                if (element.part != Entity.Null && currentPart != Entity.Null)
                {
                    throw new Exception("Trying to attach " + element.part.GetName() + " as " + bodyPartTag.ToString() + " to " + entity.GetName() + " but place is allready ocupied by " + currentPart.GetName());
                }
                if (element.part == Entity.Null && currentPart == Entity.Null)
                {
                    if (_debug) NewDebugMessage("Trying to detach " + bodyPartTag.ToString() + " from " + entity.GetName() + " but this part is allready Null");
                    continue;
                }

                field.SetValue(anatomyComponent, element.part);

                if (element.part != Entity.Null)
                {
                    var partTarnsform = element.part.GetComponentObject<Transform>();
                    var partRenderer = element.part.GetComponentObject<RendererComponent>();
                    if (authoring.partsHolder != null && partTarnsform != null) partTarnsform.SetParent(authoring.partsHolder); //костыль
                    partTarnsform.localPosition = new Vector3(0, 0, GetInternalPartOffestZ(bodyPartTag));
                    partTarnsform.localRotation = Quaternion.Euler(0, 0, 0);
                    partRenderer.spritesSortingLayer = "Solid";
                    partRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    if (_debug) NewDebugMessage(element.part.GetName() + " attached to " + entity.GetName());
                    ecb.AddComponent(element.part, new Parent() { Value = entity });
                    var id = element.part.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;
                    if (missingParts.Contains(new MissingBodypartBufferElement(bodyPartTag, id)))
                    {
                        missingParts.Remove(new MissingBodypartBufferElement(bodyPartTag, id));
                    }
                    if (element.part.HasComponent<DecayComponent>())
                    {
                        ecb.RemoveComponent<DecayComponent>(element.part);
                    }
                }
                else
                {
                    var partTarnsform = currentPart.GetComponentObject<Transform>();
                    var partRenderer = currentPart.GetComponentObject<RendererComponent>();

                    if (entity.HasComponent<AliveTag>() && !entity.HasComponent<PerformingSurgeryTag>())
                    {
                        if (bodyPartTag == BodyPartTag.Head && !PerksTree.HasPerk(entity, PerkName.Acephalos))
                        {
                            ecb.AddComponent(entity, new KillCreatureComponent(element.responsibleEntity));
                        }
                        if (bodyPartTag == BodyPartTag.LowerBody)
                        {
                            ecb.AddComponent(entity, new KillCreatureComponent(element.responsibleEntity));
                        }
                    }

                    partTarnsform.SetParent(null);
                    partTarnsform.localPosition = new Vector3(0, 0, GetInternalPartOffestZ(bodyPartTag));
                    partTarnsform.localRotation = Quaternion.Euler(0, 0, 0);
                    partRenderer.spritesSortingLayer = "Drop";
                    missingParts.Add(new MissingBodypartBufferElement(bodyPartTag, currentPart.GetComponentData<SimpleObjectNameComponent>().simpleObjectName));

                    ecb.RemoveComponent<Parent>(currentPart);
                    ecb.AddComponent(currentPart, new MoveComponent(currentTileComponent.currentTileId.ToTileData(), currentTileComponent.currentTileId.ToTileData(), MovemetType.Forced));
                    ecb.SetComponent(currentPart, currentTileComponent);
                    ecb.AddComponent(entity, new SpillLiquidComponent(1, 3));
                    ecb.AddBufferElement(entity, new MoraleChangeElement(-30));
                    ecb.RemoveComponent<PerformingSurgeryTag>(entity);

                    if (currentPart.HasComponent<InternalLiquidComponent>())
                    {
                        ecb.AddBufferElement(currentPart, new EffectElement(EffectName.Bleeding, 30, element.responsibleEntity));
                    }
                    if (currentPart.HasComponent<DecayableTag>())
                    {
                        ecb.AddComponent(currentPart, new DecayComponent(DecaySystem.baseDecayTime));
                    }
                    if (entity.HasComponent<InternalLiquidComponent>())
                    {
                        ecb.AddBufferElement(entity, new EffectElement(EffectName.Bleeding, 100, element.responsibleEntity));
                    }

                    if (_debug) NewDebugMessage(currentPart.GetName() + " detached from " + entity.GetName());
                    SoundSystem.ScheduleSound(SoundName.LoosePart, currentTileComponent.CurrentTile);
                }
                var reference = (object)anatomyComponent;
                field.SetValue(reference, element.part);

                anatomyComponent = (AnatomyComponent)reference;
            }
            if (entity.IsPlayer())
            {
                ScheduleTriggerEvent(OnPlayerAnatomyChanged);
            }
        }).WithoutBurst().Run();
        UpdateECB();

        ecb = CreateEntityCommandBuffer();

        Entities.ForEach((Entity entity, ref DynamicBuffer<AnatomyChangeElement> anatomyChangesBuffer, in EquipmentComponent equipmentComponent, in AnatomyComponent anatomyComponent) =>
        {
            foreach (var element in anatomyChangesBuffer)
            {
                foreach (var item in equipmentComponent.GetEquipmentNotNull())
                {
                    var equipTag = equipmentComponent.GetEquipTag(item);
                    if (!anatomyComponent.CanHold(equipTag))
                    {
                        ecb.AddBufferElement(entity, new ChangeEquipmentElement(Entity.Null, equipTag));
                        ecb.AddComponent(item, new ImpulseComponent(random.NextFloat2Direction(), -10, 3, element.responsibleEntity));
                    }
                }
                ecb.RemoveComponent<AnatomyChangeElement>(entity);
            }
        }).WithoutBurst().Run();
        UpdateECB();
        WriteDebug();
        TriggerEvents();
    }

    private static float GetInternalPartOffestZ(BodyPartTag bodyPartTag) => (bodyPartTag) switch
    {
        BodyPartTag.Head => -2,
        _ => 0,
    };
}

[System.Serializable]
public struct BodyPartComponent : IComponentData
{
    public BodyPartTag bodyPartTag;

    public BodyPartComponent(BodyPartTag bodyPartTag)
    {
        this.bodyPartTag = bodyPartTag;
    }

    public BodyPartComponent(SimpleObjectsTable.Param param)
    {
        this.bodyPartTag = param.bodyPartTag.DecodeCharSeparatedEnumsAndGetFirst<BodyPartTag>();
    }
}

public struct AnatomyChangeElement : IBufferElementData
{
    public Entity part;
    public BodyPartTag BodyPartTag;
    public Entity responsibleEntity;

    public AnatomyChangeElement(Entity part, BodyPartTag bodyPartTag, Entity responsibleEntity)
    {
        this.part = part;
        this.BodyPartTag = bodyPartTag;
        this.responsibleEntity = responsibleEntity;
    }

    public AnatomyChangeElement(Entity part, Entity responsibleEntity)
    {
        this.part = part;
        this.BodyPartTag = BodyPartTag.Any;
        this.responsibleEntity = responsibleEntity;
    }
}

public struct PerformingSurgeryTag : IComponentData
{
}