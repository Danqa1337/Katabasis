using Gods.GodBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using UnityEngine;

public static class GodsGenerator
{
    public static string[] Syllables = new string[]
    {
        "ar",
        "ag",
        "an",
        "ash",
        "ast",
        "as",
        "ba",
        "bo",
        "bu",
        "bra",
        "ber",
        "bur",
        "bug",
        "cur",
        "cor",
        "dor",
        "dar",
        "dir",
        "da",
        "do",
        "du",
        "don",
        "dis",
        "ef",
        "er",
        "eg",
        "est",
        "fet",
        "fer",
        "far",
        "fra",
        "ga",
        "gur",
        "gor",
        "gu",
        "gha",
        "gash",
        "hor",
        "hon",
        "hash",
        "his",
        "is",
        "il",
        "kas",
        "kag",
        "ka",
        "ku",
        "kwe",
        "kir",
        "le",
        "la",
        "lag",
        "lor",
        "mok",
        "mur",
        "me",
        "mer",
        "mog",
        "mok",
        "tur",
        "pun",
        "ru",
        "re",
        "ki",
        "ku",
        "pa",
        "gar",
        "tar",
        "gor",
    };

    public static RandomizedGod[] GenerateGods()
    {
        var gods = new List<RandomizedGod>();

        foreach (var archetype in GodsArchetypesDatabase.instance.ArchetypesByNames.Keys)
        {
            var godsOfThisArchetype = KatabasisUtillsClass.Chance(5) ? 3 : KatabasisUtillsClass.Chance(20) ? 2 : 1;

            for (int i = 0; i < godsOfThisArchetype; i++)
            {
                var index = 0;
                do
                {
                    index = UnityEngine.Random.Range(100, 200);
                }
                while (gods.Any(god => god != null && god.Index == index));

                gods.Add(GenerateGod(archetype, index));
            }
        }

        return gods.ToArray();
    }

    private static RandomizedGod GenerateGod(GodArchetype godArchetype, int index)
    {
        var archetypeData = GodsArchetypesDatabase.instance.GetGodArchetypeData(godArchetype);
        var iconData = GenerateIconData(archetypeData);
        var preferences = GenerateGodsPreferences(archetypeData);
        var name = GenerateName(archetypeData);
        var perks = GenerateGodPerks(archetypeData);
        var behaviours = GenerateGodBehavioursData(archetypeData);
        var incarnationData = GetGodIncarnationData(archetypeData);
        var god = new RandomizedGod(godArchetype, preferences, perks, behaviours, incarnationData, iconData, index, name);
        god.SetAttention(50);
        return god;
    }

    private static GodPercsData GenerateGodPerks(GodArchetypeData archetypeData)
    {
        var godTiers = new GodPercsData.PerksTier[10];
        for (int i = 0; i < 10; i++)
        {
            var archetypeTier = archetypeData.GodPercsData.GetPerksTier(i);
            var perks = archetypeTier.Perks;
            godTiers[i] = new GodPercsData.PerksTier(perks);
        }

        return new GodPercsData(godTiers);
    }

    private static RandomizedGodIconData GenerateIconData(GodArchetypeData archetypeData)
    {
        var middleColor = archetypeData.colors.RandomItem().RandomShift(0.05f);
        var topColor = middleColor.RandomShift(0.1f);
        var bottomColor = middleColor.RandomShift(0.1f);

        var topPart = archetypeData.tops.RandomItem();
        var midPart = archetypeData.mids.RandomItem();
        var botPart = archetypeData.bots.RandomItem();

        var top = new KeyValuePair<TopGodIconPart, Color>(topPart, topColor);
        var mid = new KeyValuePair<MiddleGodIconPart, Color>(midPart, middleColor);
        var bot = new KeyValuePair<BottomGodIconPart, Color>(botPart, bottomColor);

        return new RandomizedGodIconData(top, mid, bot);
    }

    private static RandomizedGod.GodsPreferences GenerateGodsPreferences(GodArchetypeData archetypeData)
    {
        var likedTags = archetypeData.sacrificeTags;
        var sacrificeWays = archetypeData.sacrificeWays;

        return new RandomizedGod.GodsPreferences(likedTags, sacrificeWays);
    }

    private static GodBehavioursData GenerateGodBehavioursData(GodArchetypeData archetypeData)
    {
        var archetypeBehavioursData = archetypeData.GodBehavioursData;

        var pBehaviours = new List<GodBehaviourName> { GodBehaviourName.None };
        var gBehaviours = new List<GodBehaviourName> { GodBehaviourName.None };
        var eBehaviours = new List<GodBehaviourName> { GodBehaviourName.None };

        if (archetypeBehavioursData.PrayerBehaviourNames.Length > 0) pBehaviours.AddRange(archetypeBehavioursData.PrayerBehaviourNames.RandomItems(3));
        if (archetypeBehavioursData.GoodBehaviourNames.Length > 0) gBehaviours.AddRange(archetypeBehavioursData.GoodBehaviourNames.RandomItems(3));
        if (archetypeBehavioursData.EvilBehaviourNames.Length > 0) eBehaviours.AddRange(archetypeBehavioursData.EvilBehaviourNames.RandomItems(3));

        return new GodBehavioursData(pBehaviours.ToArray(), gBehaviours.ToArray(), eBehaviours.ToArray());
    }

    private static GodIncarnationData GetGodIncarnationData(GodArchetypeData godArchetypeData)
    {
        var incarnate = godArchetypeData.GodIncarnationData.Incarnate;
        var lesser = godArchetypeData.GodIncarnationData.LesserServats.RandomItems(3);
        var grether = godArchetypeData.GodIncarnationData.GreatherServats.RandomItems(3);

        return new GodIncarnationData(incarnate, lesser, grether);
    }

    private static string GenerateName(GodArchetypeData archetypeData)
    {
        var sillablesCount = KatabasisUtillsClass.Chance(10) ? 1 : KatabasisUtillsClass.Chance(40) ? 3 : 2;
        var syllables = new string[sillablesCount];
        var hyphenUsed = false;
        for (int i = 0; i < syllables.Length; i++)
        {
            if (i > 1 && KatabasisUtillsClass.Chance(10))
            {
                syllables[i] = syllables[i - 1];
            }
            else
            {
                syllables[i] = Syllables.RandomItem();
            }
            if (!hyphenUsed && i != syllables.Length - 1 && KatabasisUtillsClass.Chance(8))
            {
                syllables[i] += "-";
                hyphenUsed = true;
            }
        }

        string name = "";
        for (int i = 0; i < syllables.Length; i++)
        {
            name += syllables[i];
        }
        name = name.FirstCharToUpper();
        return name;
    }
}

public static class Transcriptor
{
    public static string TranscribeToCyrillic(string text)
    {
        var stringBuilder = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            var letter = text[i];
            var transcribedLetter = ToCyrillic(char.ToLower(letter));
            if (char.IsUpper(letter))
            {
                transcribedLetter = Char.ToUpper(transcribedLetter);
            }
            stringBuilder.Append(transcribedLetter);
        }

        return stringBuilder.ToString();
    }

    public static char ToCyrillic(char letter)
    {
        switch (letter)
        {
            case 'a': return 'а';
            case 'b': return 'б';
            case 'c': return 'ц';
            case 'd': return 'д';
            case 'e': return 'е';
            case 'f': return 'ф';
            case 'g': return 'г';
            case 'h': return 'х';
            case 'i': return 'и';
            case 'j': return 'ж';
            case 'k': return 'к';
            case 'l': return 'л';
            case 'm': return 'м';
            case 'n': return 'н';
            case 'o': return 'о';
            case 'p': return 'п';
            case 'q': return 'к';
            case 'r': return 'р';
            case 's': return 'с';
            case 't': return 'т';
            case 'u': return 'у';
            case 'v': return 'в';
            case 'w': return 'в';
            case 'x': return 'ц';
            case 'y': return 'ю';
            case 'z': return 'з';
            default: return letter;
        }
    }
}