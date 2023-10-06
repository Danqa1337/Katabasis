using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public static class EntityExtensions
{
    public static bool IsPlayer(this Entity entity)
    {
        return entity.HasComponent<PlayerTag>();
    }

    public static bool Exists(this Entity entity)
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity);
    }

    public static bool IsPlayersSquadmate(this Entity entity)
    {
        return entity != Entity.Null && !entity.IsPlayer() && entity.HasComponent<SquadMemberComponent>() && Registers.SquadsRegister.GetSquadIndex(entity) == 1;
    }

    public static bool IsInInvis(this Entity entity)
    {
        if (entity.HasBuffer<EffectElement>())
        {
            foreach (var item in entity.GetBuffer<EffectElement>())
            {
                if (item.EffectName == EffectName.Invisibility)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool HasEffect(this Entity entity, EffectName type)
    {
        if (entity.HasBuffer<EffectElement>())
        {
            foreach (var item in entity.GetBuffer<EffectElement>())
            {
                if (item.duration != 0 && item.EffectName == type)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool HasTag(this Entity entity, Tag tag)
    {
        return entity.GetTags().Contains(tag);
    }
    public static bool HasTag(this Entity entity, Tag tag, EntityManager entityManager)
    {
        return entity.GetTags().Contains(tag);
    }

    public static bool HasComponent<T>(this Entity entity)
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<T>(entity);
    }
    public static bool HasComponent<T>(this Entity entity, EntityManager entityManager)
    {
        return entityManager.HasComponent<T>(entity);
    }

    public static bool HasBuffer<T>(this Entity entity) where T : unmanaged, IBufferElementData
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<T>(entity);
    }
    public static bool HasBuffer<T>(this Entity entity, EntityManager entityManager) where T : unmanaged, IBufferElementData
    {
        return entityManager.HasComponent<T>(entity);
    }

    public static bool CanBeSwapedRightNow(this Entity passiveEntity, Entity activeEntity)
    {
        if (passiveEntity.CanBeSwapedPotentialy(activeEntity)) //is not null
        {
            if (activeEntity.IsPlayer())
            {
                return true;
            }
            else
            {
                var passiveAi = passiveEntity.GetComponentData<AIComponent>();
                var activeAi = activeEntity.GetComponentData<AIComponent>();

                if (passiveAi.target != activeAi.target)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool CanBeSwapedPotentialy(this Entity passiveEntity, Entity activeEntity)
    {
        if (passiveEntity != Entity.Null) //is not null
        {
            if (passiveEntity.HasComponent<AIComponent>()) //is creature
            {
                if (!passiveEntity.IsHostile(activeEntity)) //is not hostile
                {
                    var passiveAi = passiveEntity.GetComponentData<AIComponent>();
                    if (Registers.SquadsRegister.AreSquadmates(activeEntity, Player.PlayerEntity))
                    {
                        return true;
                    }
                    else
                    {
                        if (passiveAi.target == Entity.Null)
                        {
                            return true;
                        }
                        else
                        {
                            if (!passiveEntity.HasEffect(EffectName.EngagedInBattle))
                            {
                                if (activeEntity.IsPlayer())
                                {
                                    return true;
                                }
                                else
                                {
                                    var activeAi = activeEntity.GetComponentData<AIComponent>();

                                    if (!(activeAi.isFleeing && passiveAi.isFleeing)) //at least one creature is not fleeing
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public static bool IsHostile(this Entity self, Entity entity)
    {
        if (self != Entity.Null && entity != Entity.Null)
        {
            if (entity.HasComponent<AIComponent>())
            {
                if (!Registers.SquadsRegister.AreSquadmates(self, entity))
                {
                    if (Registers.SquadsRegister.AreSquadsEnemies(Registers.SquadsRegister.GetSquadIndex(self), Registers.SquadsRegister.GetSquadIndex(entity)))
                    {
                        return true;
                    }

                    if (self.HasComponent<AIComponent>())
                    {
                        var selfHostileTags = self.GetComponentData<AIComponent>().GetHostileTags();

                        foreach (var tag in entity.GetTags())
                        {
                            if (selfHostileTags.Contains(tag))
                            {
                                return true;
                            }
                        }
                    }

                    var hostileTags = entity.GetComponentData<AIComponent>().GetHostileTags();

                    foreach (var tag in self.GetTags())
                    {
                        if (hostileTags.Contains(tag))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public static SimpleObjectName GetItemName(this Entity entity)
    {
        if (entity.HasComponent<SimpleObjectNameComponent>())
        {
            return entity.GetComponentData<SimpleObjectNameComponent>().simpleObjectName;
        }
        return SimpleObjectName.Null;
    }

    public static TileData CurrentTile(this Entity entity)
    {
        if (entity == Entity.Null)
        {
            throw new NullReferenceException();
        }
        else
        {
            if (entity.HasComponent<BodyPartComponent>() && entity.HasComponent<Parent>())
            {
                var root = entity.GetComponentData<Parent>().Value;

                return root.GetComponentData<CurrentTileComponent>().currentTileId.ToTileData();
            }
            else
            {
                return entity.GetComponentData<CurrentTileComponent>().currentTileId.ToTileData();
            }
        }
    }

    public static HashSet<Tag> GetTags(this Entity self)
    {
        var list = new HashSet<Tag>();
        foreach (var item in self.GetBuffer<TagBufferElement>())
        {
            list.Add(item.tag);
        }
        return list;
    }
    public static HashSet<Tag> GetTags(this Entity self, EntityManager entityManager)
    {
        var list = new HashSet<Tag>();
        foreach (var item in self.GetBuffer<TagBufferElement>(entityManager))
        {
            list.Add(item.tag);
        }
        return list;
    }

    public static HashSet<EffectElement> GetEffects(this Entity entity)
    {
        var effects = new HashSet<EffectElement>();
        if (entity.HasBuffer<EffectElement>())
        {
            foreach (var item in entity.GetBuffer<EffectElement>())
            {
                effects.Add(item);
            }
        }
        return effects;
    }

    public static NativeArray<T> Shuffle<T>(this NativeArray<T> list, Unity.Mathematics.Random rng) where T : unmanaged
    {
        int n = list.Length;
        while (n > 1)
        {
            n--;
            int k = rng.NextInt(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public static NativeArray<Tag> GetTagsNative(this Entity self, Allocator allocator)
    {
        var buffer = self.GetBuffer<TagBufferElement>();
        var array = new NativeArray<Tag>(buffer.Length, allocator);

        for (int i = 0; i < buffer.Length; i++)
        {
            array[i] = buffer[i].tag;
        }

        return array;
    }

    public static NativeArray<Tag> GetHostileTagsNative(this Entity self, Allocator allocator)
    {
        var buffer = self.GetBuffer<EnemyTagBufferElement>();
        var array = new NativeArray<Tag>(buffer.Length, allocator);

        for (int i = 0; i < buffer.Length; i++)
        {
            array[i] = buffer[i].tag;
        }

        return array;
    }

    public static Vector3 GetTilePositionOffset(this Entity entity)
    {
        if ((entity.HasComponent<AIComponent>() || entity.IsPlayer()) && entity.GetObjectType() == ObjectType.Solid)
        {
            return new Vector3(0, 0.2f, 0);
        }
        else
        {
            return Vector3.zero;
        }
    }

    public static ObjectType GetObjectType(this Entity entity)
    {
        return entity.GetComponentData<ObjectTypeComponent>().objectType;
    }

    public static string GetName(this Entity entity)
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.GetName(entity);
    }
    public static string GetName(this Entity entity, EntityManager entityManager)
    {
        return entityManager.GetName(entity) + " " +entity.Index;
    }

    public static DynamicBuffer<T> AddBuffer<T>(this Entity entity) where T : unmanaged, IBufferElementData
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<T>(entity);
    }

    public static DynamicBuffer<T> GetBuffer<T>(this Entity entity, bool isReadOnly = false) where T : unmanaged, IBufferElementData
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<T>(entity, isReadOnly);
    }
    public static DynamicBuffer<T> GetBuffer<T>(this Entity entity, EntityManager entityManager, bool isReadOnly = false) where T : unmanaged, IBufferElementData
    {
        return entityManager.GetBuffer<T>(entity, isReadOnly);
    }

    public static T GetComponentObject<T>(this Entity entity)
    {
        return World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentObject<T>(entity);
    }
    public static T GetComponentObject<T>(this Entity entity, EntityManager entityManager)
    {
        return entityManager.GetComponentObject<T>(entity);
    }

    public static T GetComponentData<T>(this Entity entity) where T : unmanaged, IComponentData
    {
        if (entity.HasComponent<T>())
        {
            return World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<T>(entity);
        }
        throw new NullReferenceException("no such component " + typeof(T).ToString() + " on " + entity.GetName());
    }
    public static T GetComponentData<T>(this Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
    {
        if (entity.HasComponent<T>(entityManager))
        {
            return entityManager.GetComponentData<T>(entity);
        }
        throw new NullReferenceException("no such component " + typeof(T).ToString() + " on " + entity.GetName(entityManager));
    }

    //voids

    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> list)
    {
        foreach (var item in list)
        {
            set.Add(item);
        }
    }

    public static void SetComponentData<T>(this Entity entity, T data) where T : unmanaged, IComponentData
    {
        if (entity == Entity.Null) throw new System.NullReferenceException();
        if (entity.HasComponent<T>())
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(entity, data);
        }
        else
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, data);
        }
    }
    public static void SetComponentData<T>(this Entity entity, T data, EntityManager entityManager) where T : unmanaged, IComponentData
    {
        if (entity == Entity.Null) throw new System.NullReferenceException();
        if (entity.HasComponent<T>(entityManager))
        {
           entityManager.SetComponentData(entity, data);
        }
        else
        {
            entityManager.AddComponentData(entity, data);
        }
    }

    public static void SetZeroSizedTagComponentData<T>(this Entity entity, T data) where T : unmanaged, IComponentData
    {
        if (entity == Entity.Null) throw new System.NullReferenceException();

        World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<T>(entity);
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, data);
    }
    public static void SetZeroSizedTagComponentData<T>(this Entity entity, T data, EntityManager entityManager) where T : unmanaged, IComponentData
    {
        if (entity == Entity.Null) throw new System.NullReferenceException();

        entityManager.RemoveComponent<T>(entity);
        entityManager.AddComponentData(entity, data);
    }

    public static void AddComponentData<T>(this Entity entity, T data, EntityManager entityManager) where T : unmanaged, IComponentData
    {
        if (entity == Entity.Null) throw new NullReferenceException();
        entityManager.AddComponentData(entity, data);
    }
    public static void AddComponentData<T>(this Entity entity, T data) where T : unmanaged, IComponentData
    {
        if (entity == Entity.Null) throw new NullReferenceException();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, data);
    }

    public static void AddBufferElement<T>(this Entity entity, T d) where T : unmanaged, IBufferElementData
    {
        if (!entity.HasBuffer<T>())
        {
            entity.AddBuffer<T>();
        }
        World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<T>(entity).Add(d);
    }
    public static void AddBufferElement<T>(this Entity entity, T d, EntityManager entityManager) where T : unmanaged, IBufferElementData
    {
        if (!entity.HasBuffer<T>())
        {
            entity.AddBuffer<T>();
        }
        entityManager.GetBuffer<T>(entity).Add(d);
    }

    public static void AddBufferElement<T>(this Entity entity, T d, EntityCommandBuffer ecb) where T : unmanaged, IBufferElementData
    {
        if (!entity.HasBuffer<T>())
        {
            ecb.AddBuffer<T>(entity);
        }
        ecb.AppendToBuffer(entity, d);
    }

    public static void AddBufferElement<T>(this Entity entity, T d, EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex) where T : unmanaged, IBufferElementData
    {
        if (!entity.HasBuffer<T>())
        {
            ecb.AddBuffer<T>(entityInQueryIndex, entity);
        }
        ecb.AppendToBuffer(entityInQueryIndex, entity, d);
    }

    public static void AddBufferElement<T>(this EntityCommandBuffer ecb, Entity entity, T d) where T : unmanaged, IBufferElementData
    {
        if (entity.Index < 0 || !entity.HasBuffer<T>())
        {
            ecb.AddBuffer<T>(entity);
        }
        ecb.AppendToBuffer(entity, d);
    }


    public static void AddOrAppendBufferElement<T>(this EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, Entity entity, T d) where T : unmanaged, IBufferElementData
    {
        if (!entity.HasBuffer<T>())
        {
            ecb.AddBuffer<T>(entityInQueryIndex, entity);
        }
        ecb.AppendToBuffer(entityInQueryIndex, entity, d);
    }

    public static void AddBufferElement<T>(this EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, Entity entity, T d) where T : unmanaged, IBufferElementData
    {
        ecb.AddBuffer<T>(entityInQueryIndex, entity);
        ecb.AppendToBuffer(entityInQueryIndex, entity, d);
    }

    public static void RemoveBuffer<T>(this Entity entity) where T : unmanaged, IBufferElementData
    {
        World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<T>(entity);
    }

    public static void AddComponentObject(this Entity entity, object obj)
    {
        if (entity == Entity.Null) throw new NullReferenceException();

        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentObject(entity, obj);
    }

    public static void RemoveComponent<T>(this Entity entity) where T : unmanaged, IComponentData
    {
        if (entity.HasComponent<T>())
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<T>(entity);
        }
    }

    public static void RemoveZeroSizedComponent<T>(this Entity entity) where T : unmanaged, IComponentData
    {
        World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<T>(entity);
    }

    public static void ShowRenderer(this Entity entity)
    {
        if (entity.HasComponent<AnatomyComponent>())
        {
            foreach (var item in entity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
            {
                item.GetComponentObject<RendererComponent>().Show();
            }
        }
    }

    public static void HideRenderer(this Entity entity)
    {
        if (entity.HasComponent<AnatomyComponent>())
        {
            foreach (var item in entity.GetComponentData<AnatomyComponent>().GetBodyPartsNotNull())
            {
                item.GetComponentObject<RendererComponent>().Hide();
            }
        }
    }

    public static void Remove<T>(this NativeList<T> list, T element) where T : unmanaged
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].Equals(element))
            {
                list.RemoveAtSwapBack(i);
                return;
            }
        }
        // Debug.Log(list.Contains(element));
    }

    public static void Remove<T>(this DynamicBuffer<T> list, T element) where T : unmanaged
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].Equals(element))
            {
                list.RemoveAtSwapBack(i);
            }
        }
    }

    public static void SetName(this Entity entity, string name)
    {
        //#if UNITY_EDITOR
        World.DefaultGameObjectInjectionWorld.EntityManager.SetName(entity, name);
        //#endif
    }

    public static void Destroy(this Entity entity)
    {
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entity);
    }

    public static Vector3 ToRealPosition(this int2 vector, Entity entity)
    {
        return new Vector3(vector.x, vector.y, vector.y * 10) + entity.GetTilePositionOffset();
    }

    public static bool Contains<T>(this DynamicBuffer<T> buffer, T item) where T : unmanaged
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Equals(item))
            {
                return true;
            }
        }
        return false;
    }
}