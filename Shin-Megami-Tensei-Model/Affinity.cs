using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;

public class Affinity
{
    public List<AbilityType> Weak { get; set; }
    public List<AbilityType> Resist { get; set; }
    public List<AbilityType> Null { get; set; }
    public List<AbilityType> Repel { get; set; }
    public List<AbilityType> Drain { get; set; }

    private static List<AffinityType> GetListOfAffinityForTeam(Unit[] units, Ability ability)
    {
        return units.Where(unit => unit != null).Select(unit => unit.Affinity.GetAffinity(ability.Type)).ToList();
    }
    
    public static AffinityType GetPrioritizeAffinity(Unit[] units, Ability ability)
    {
        var affinities = GetListOfAffinityForTeam(units, ability);
        
        AffinityType[] affinityPriority =
        {
            AffinityType.Drain, AffinityType.Repel, AffinityType.Null, AffinityType.Weak, AffinityType.Neutral,
            AffinityType.Resist
        };

        foreach (var priority in affinities)
        {
            if (affinityPriority.Contains(priority)) return priority;
        }
        return AffinityType.Neutral;
    }
    public AffinityType GetAffinity(AbilityType abilityType)
    {
        if (Weak.Contains(abilityType))
        {
            return AffinityType.Weak;
        }
        if (Resist.Contains(abilityType))
        {
            return AffinityType.Resist;
        }

        if (Null.Contains(abilityType))
        {
            return AffinityType.Null;
        }

        if (Repel.Contains(abilityType))
        {
            return AffinityType.Repel;
        }
        
        return Drain.Contains(abilityType) ? AffinityType.Drain : AffinityType.Neutral;
    }
    
    public static double AffinityModifier(AffinityType affinityType)
    {
        return affinityType switch
        {
            AffinityType.Weak => 1.5,
            AffinityType.Resist => 0.5,
            AffinityType.Null => 0.0,
            AffinityType.Drain => -1.0,
            _ => 1.0,
        };
    }
    
    
}