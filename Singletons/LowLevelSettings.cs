using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LowLevelSettings : Singleton<LowLevelSettings>
{
    [Header("Constants")]
    public float timescale = 1;
    public float baseRangedInaccuracity;
    public float spawnLevelDeviation;
    [Range(0, 1)]
    public float maxDarknessA;

    [Range(0, 1000)]
    public int majorFrameDrawInterval;

    [Range(0, 1000)]
    public int minorFrameDrawInterval;

    [Range(0, 100)]
    public int framesPerAnimation;

    [Space]
    [Header("Animations")]
    public bool playAnimations;
    public AnimationCurve MovementAnimation;
    public AnimationCurve ButtAnimation;

    [Space]
    [Header("Debug")]
    public bool showDamageOnObjects;
    public bool debugSystems;
    public bool debugAI;
    public bool debugSquads;

    public bool debugMovement;
    public bool debugPathfinding;
    public bool debugTargetSearch;
    public bool debugAbilities;
    public bool debugCollisions;
    public bool debugEquip;
    public bool debugInventory;
    public bool debugAnatomy;
    public bool debugPhysics;

    [Space]

    [Header("Textures")]
    public Texture2D FOV;
    [SerializeField] private Texture2D PathMask;

    public Sprite missingTexture;

    public bool useAtlases;
    public Material defaultMaterial;

}