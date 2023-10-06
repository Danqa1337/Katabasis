using System;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleObjectsSpawner : SpawnerUtility
{
    public Entity Spawn(SimpleObjectData simpleObjectData, TileData currentTile)
    {
        try
        {
            var tags = simpleObjectData.tagElements.Select(t => t.tag).ToArray();
            var objectType = simpleObjectData.objectTypeComponent.objectType;
            var itemName = simpleObjectData.SimpleObjectName;
            var spritesCollection = SpriteCollectionsDatabase.GetSpriteCollection(itemName);

            Sprite sprite = null;
            Transform transform = null;
            RendererComponent renderer = null;
            EntityAuthoring authoring = null;

            if (objectType == ObjectType.Floor || objectType == ObjectType.Liquid)
            {
                var rndSpriteNum = simpleObjectData.rndSpriteNum;
                if (!spritesCollection.hasSeamlessTexture)
                {
                    if (rndSpriteNum == -1)
                    {
                        rndSpriteNum = Random.Range(0, spritesCollection.sprites.Count);
                    }

                    if (simpleObjectData.altSpriteDrown)
                    {
                        sprite = spritesCollection.alternativeSprites[rndSpriteNum];
                    }
                    else
                    {
                        sprite = spritesCollection.sprites[rndSpriteNum];
                    }
                }
                else
                {
                    sprite = spritesCollection.sprites[30];
                }

                FloorBaker.DrawTile(currentTile.position, sprite, objectType);
            }
            else
            {
                authoring = Pooler.Take("SimpleObject", Vector3.zero).GetComponent<EntityAuthoring>();
                transform = authoring.transform;
                renderer = authoring.bodyRenderer;

                if (!renderer.gameObject.activeSelf) renderer.gameObject.SetActive(true);
                transform.position = currentTile.position.ToRealPosition();
                transform.rotation = quaternion.Euler(Vector3.zero);
                transform.localScale = new Vector3(1, 1, 1);
                renderer.ResetAll();
                renderer.DrawCollection(spritesCollection, simpleObjectData.rndSpriteNum, simpleObjectData.altSpriteDrown);

                SortingLayer sortingLayer = simpleObjectData.sortingLayer;

                if (sortingLayer == SortingLayer.Default)
                {
                    sortingLayer = objectType.ToString().DecodeCharSeparatedEnumsAndGetFirst<SortingLayer>();
                }
                renderer.spritesSortingLayer = sortingLayer.ToString();
                authoring.name = itemName.ToString();
            }

            if (Application.isPlaying)
            {
                var entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();

                entity.SetName(itemName.ToString());

                if (spritesCollection.hasSeamlessTexture) entity.AddComponentData(new SeamlessTextureTag());

                AddBaseComponents();

                ProcessTags();

                currentTile.Save();
                currentTile.Add(entity, objectType);
                if (renderer != null)
                {
                    var animator = renderer.GetComponent<Animator>();
                    animator.runtimeAnimatorController = AnimationsDatabase.GetAnimationData(simpleObjectData.SimpleObjectName).animatorController;
                }

                return entity;

                //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                void AddBaseComponents()
                {
                    if (renderer != null) entity.AddComponentObject(renderer);
                    if (transform != null) entity.AddComponentObject(transform);
                    if (authoring != null) entity.AddComponentObject(authoring);

                    entity.AddComponentData(new SimpleObjectNameComponent(simpleObjectData.SimpleObjectName));
                    entity.AddComponentData(simpleObjectData.objectTypeComponent);
                    entity.AddComponentData(new LocalTransform());
                    entity.AddComponentData(new LocalToWorld());
                    entity.AddComponentData(new SceneSystemData());
                    entity.AddComponentData(new CurrentTileComponent(currentTile.index));

                    entity.AddBuffer<OverHeadAnimationElement>();

                    AddComponent(entity, simpleObjectData.objectSoundData);
                    AddComponent(entity, simpleObjectData.physicsComponent);
                    AddComponent(entity, simpleObjectData.durabilityComponent);
                    AddComponent(entity, simpleObjectData.impulseComponent);
                    AddComponent(entity, simpleObjectData.bodyPartComponent);
                    AddComponent(entity, simpleObjectData.rangedWeaponComponent);
                    AddComponent(entity, simpleObjectData.doorComponent);
                    AddComponent(entity, simpleObjectData.lockComponent);
                    AddComponent(entity, simpleObjectData.keyComponent);
                    AddComponent(entity, simpleObjectData.decayComponent);
                    AddComponent(entity, simpleObjectData.internalLiquidComponent);

                    AddBuffer(entity, simpleObjectData.tagElements);
                    AddBuffer(entity, simpleObjectData.dropElements);
                    AddBuffer(entity, simpleObjectData.effectsOnHit);
                    AddBuffer(entity, simpleObjectData.effectsOnApplying);
                    AddBuffer(entity, simpleObjectData.activeEffects);

                    if (simpleObjectData.lockComponent.IsValid)
                    {
                        if (simpleObjectData.lockComponent.Component.lockIndex == -1)
                        {
                            entity.SetComponentData<LockComponent>(new LockComponent(Registers.LocksRegister.RegisterNewLock()));
                        }
                    }
                    else if (simpleObjectData.doorComponent.IsValid && currentTile.Template.isCoridorBlock)
                    {
                        entity.SetComponentData<LockComponent>(new LockComponent(Registers.LocksRegister.RegisterNewLock()));
                    }
                }
                void ProcessTags()
                {
                    if (tags.Contains(Tag.Applyable))
                    {
                        entity.AddComponentData(new ApplyableComponent(entity));
                    }
                    if (tags.Contains(Tag.Mapable) || objectType == ObjectType.GroundCover)
                    {
                        entity.AddComponentData(new MapableTag());
                    }
                    if (tags.Contains(Tag.Cloud))
                    {
                        entity.AddComponentData(new CloudComponent(100));
                    }
                    if (tags.Contains(Tag.Fire))
                    {
                        currentTile.isOnFire = true;
                        entity.AddComponentData(new FireComponent());
                    }
                    if (tags.Contains(Tag.TickAnimated))
                    {
                        entity.AddComponentData(new OnTickAnimationComponent());
                    }
                    if (tags.Contains(Tag.Amphibious))
                    {
                        entity.AddComponentData(new AmphibiousTag());
                    }

                    if (tags.Contains(Tag.PressurePlate))
                    {
                        entity.AddComponentData(new PressurePlateComponent());
                    }

                    if (tags.Contains(Tag.Projectile))
                    {
                        entity.AddComponentData(new ProjectileTag());
                    }

                    if (tags.Contains(Tag.LOSblock)) entity.AddComponentData(new LOSBlockTag());

                    if (tags.Contains(Tag.Digger)) entity.AddComponentData(new DiggerTag());

                    if (tags.Contains(Tag.Flying)) entity.AddComponentData(new FlyingTag());

                    if (tags.Contains(Tag.Unmovable))
                    {
                        entity.AddComponentData(new UnmovableTag());
                    }

                    if (tags.Contains(Tag.Dummy)) entity.AddComponentData(new DummyTag());

                    if (tags.Contains(Tag.Polearm)) entity.AddComponentData(new PolearmTag());

                    if (tags.Contains(Tag.Unique))
                    {
                        entity.AddComponentData(new UniqueTag());
                        //Registers.UniqueObjectsRegister.Set(itemName, true, true);
                    }
                    if (tags.Contains(Tag.Slave))
                    {
                        entity.SetZeroSizedTagComponentData(new SlaveTag());
                    }
                    if (tags.Contains(Tag.Slaver))
                    {
                        entity.SetComponentData(new SlaverComponent(entity));
                    }
                    if (tags.Contains(Tag.Door))
                    {
                        entity.AddComponentData(new MechanismTag());
                    }
                    if (tags.Contains(Tag.Explosive))
                    {
                        entity.SetZeroSizedTagComponentData(new ExplosiveTag());
                    }
                    if (tags.Contains(Tag.ExtraFragile))
                    {
                        entity.SetZeroSizedTagComponentData(new ExtraFragileTag());
                    }
                    if (tags.Contains(Tag.Decayable))
                    {
                        entity.SetZeroSizedTagComponentData(new DecayableTag());
                    }
                }
            }

            return Entity.Null;
        }
        catch (Exception exception)
        {
            Debug.Log("Failed to spawn " + simpleObjectData.SimpleObjectName + ": " + exception.ToString());
            throw exception;
        }
    }
}
