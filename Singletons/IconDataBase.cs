using Abilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

public class IconDataBase : Database<IconDataBase, IconDataBase, IconDataBase, IconDataBase>
{
    public Sprite bleedingIcon;
    public Sprite stunIcon;
    public Sprite burningIcon;
    public Sprite HasteIcon;
    public Sprite SlownessIcon;
    public Sprite EngagedIcon;
    public Sprite RegenerationIcon;
    public Sprite PoisonIcon;
    public Sprite InvisIcon;
    public Sprite PetrificationIcon;
    public Sprite ArmourIgnoringIcon;
    public Sprite FearIcon;
    public Sprite InteractionIcon;
    public Sprite NoAmmoIcon;
    public Sprite DeathIcon;
    public Sprite ParryIcon;
    public Sprite EvasionIcon;
    public Sprite JumpIcon;
    public Sprite YesIcon;
    public Sprite NoIcon;
    public Sprite LockedIcon;
    public Sprite UnlockedIcon;
    public Sprite TravelIcon;

    private SpriteAtlas _actionIconAtlas;
    private SpriteAtlas _godIconAtlas;
    private SpriteAtlas _perksAtals;
    private SpriteAtlas _effectsAtlas;
    private SpriteAtlas _godIconPartsAtlas;

    private Sprite GetPopupIcon_(PopupType type) => (type) switch
    {
        PopupType.Fear => FearIcon,
        PopupType.Interaction => InteractionIcon,
        PopupType.Bleeding => bleedingIcon,
        PopupType.NoAmmo => NoAmmoIcon,
        PopupType.Death => DeathIcon,
        PopupType.Parry => ParryIcon,
        PopupType.Evade => EvasionIcon,
        PopupType.jump => JumpIcon,
        PopupType.Healing => RegenerationIcon,
        PopupType.No => NoIcon,
        PopupType.Yes => YesIcon,
        PopupType.Locked => LockedIcon,
        PopupType.Unlocked => UnlockedIcon,
        _ => null,
    };

    public static Sprite GetEffectIcon(EffectName effectName)
    {
        return instance._effectsAtlas.GetSprite(effectName.ToString() + "EffectIcon");
    }

    public static Sprite GetAbilityIcon(AbilityName ability)
    {
        return instance._actionIconAtlas.GetSprite(ability.ToString() + "AbilityIcon");
    }

    public static Sprite GetPerkIcon(PerkName perkName)
    {
        return instance._perksAtals.GetSprite(perkName.ToString() + "PerkIcon");
    }

    public static Sprite GetTopGodIconPart(TopGodIconPart godIconPart)
    {
        return instance._godIconPartsAtlas.GetSprite(godIconPart.ToString());
    }

    public static Sprite GetMidGodIconPart(MiddleGodIconPart godIconPart)
    {
        return instance._godIconPartsAtlas.GetSprite(godIconPart.ToString());
    }

    public static Sprite GetBotGodIconPart(BottomGodIconPart godIconPart)
    {
        return instance._godIconPartsAtlas.GetSprite(godIconPart.ToString());
    }

    public static Sprite GetGodIcon(GodArchetype GodArchetype)
    {
        return instance._godIconAtlas.GetSprite(GodArchetype.ToString() + "GodIcon");
    }

    public static Sprite GetActionIcon(ControllerActionName controllerAction)
    {
        return instance._actionIconAtlas.GetSprite(controllerAction.ToString() + "AbilityIcon");
    }

    public static Sprite GetPopupIcon(PopupType type)
    {
        return instance.GetPopupIcon_(type);
    }

    public override void StartUp()
    {
        instance._actionIconAtlas = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/Resources_moved/Sprites/ActionIcons/ActionIcons.spriteatlas").WaitForCompletion();
        instance._godIconAtlas = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/Resources_moved/Sprites/GodIcons/Gods.spriteatlas").WaitForCompletion();
        instance._perksAtals = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/Resources_moved/Sprites/PerkIcons/PerkIcons.spriteatlas").WaitForCompletion();
        instance._effectsAtlas = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/Resources_moved/Sprites/EffectsIcons/EffectsIcons.spriteatlas").WaitForCompletion();
        instance._godIconPartsAtlas = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/Resources_moved/Sprites/GodIcons/Parts/GodIconsParts.spriteatlas").WaitForCompletion();
    }

    protected override void ProcessParam(IconDataBase param)
    {
        throw new System.NotImplementedException();
    }
}