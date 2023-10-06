using Gods;
using System;
using System.Linq;

[Serializable]
public class RandomizedGod : God
{
    private readonly GodArchetype _godArchetype;
    private int _index;
    private string _name;

    public readonly RandomizedGodIconData IconData;
    public override GodArchetype GodArchetype => _godArchetype;
    public override int Index => _index;

    public override string Name
    {
        get
        {
            if (LocalizationManager.CurrentLocaliztion == LocalizationVariant.Russian)
            {
                return Transcriptor.TranscribeToCyrillic(_name);
            }
            return _name;
        }
    }

    public RandomizedGod(GodArchetype godArchetype, GodsPreferences preferences, GodPercsData godPercsData, GodBehavioursData godBehavioursData, GodIncarnationData godIncarnationData, RandomizedGodIconData iconData, int index, string name)
    {
        _godArchetype = godArchetype;
        _preferences = preferences;
        IconData = iconData;
        _godIncarnationData = godIncarnationData;
        _index = index;
        _name = name;
        _godPercsData = godPercsData;
        _godBehavioursData = godBehavioursData;
    }

    public override bool CanAcceptSacrifice(SacrificeData sacrificeData)
    {
        var wayMatch = Preferences.WaysOfSacrifices.Any(w => w == sacrificeData.DamageType);
        if (wayMatch)
        {
            var tags = sacrificeData.Entity.GetTags();
            var tagsMatch = Preferences.LikedTags.Any(t => tags.Contains(t));
            return tagsMatch;
        }
        return false;
    }

    [System.Serializable]
    public class GodsPreferences
    {
        public readonly Tag[] LikedTags;
        public readonly DurabilityChangeReason[] WaysOfSacrifices;

        public GodsPreferences(Tag[] likedTags, DurabilityChangeReason[] waysOfSacrifices)
        {
            LikedTags = likedTags;
            WaysOfSacrifices = waysOfSacrifices;
        }
    }
}