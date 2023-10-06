using System;
using UnityEngine;

[Serializable]
public class GodArchetypeData
{
    public GodArchetype GodArchetype;
    public DurabilityChangeReason[] sacrificeWays;
    public Tag[] sacrificeTags;
    public TopGodIconPart[] tops;
    public MiddleGodIconPart[] mids;
    public BottomGodIconPart[] bots;
    public Color[] colors;

    public GodPercsData GodPercsData;
    public GodIncarnationData GodIncarnationData;
    public GodBehavioursData GodBehavioursData;
    public bool isUnrandom;
}