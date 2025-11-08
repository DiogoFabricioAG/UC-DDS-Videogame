using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;

public class Affinity
{
    private const double WeakModifierAttack = 1.5;
    private const double ResistModifierAttack = 0.5;
    private const double NullModifierAttack = 0.0;
    private const double NeutralModifierAttack = 1.0;
    private const double DrainModifierAttack = -1.0;
    public List<AbilityType> Weak { get; set; }
    public List<AbilityType> Resist { get; set; }
    public List<AbilityType> Null { get; set; }
    public List<AbilityType> Repel { get; set; }
    public List<AbilityType> Drain { get; set; }

    private static List<AffinityType> GetListOfAffinityForTeam(Unit[] units, Ability ability)
    {
        return units.Where(unit => unit != null).Select(unit => unit.Affinity.GetAffinity(ability.Type)).ToList();
    }
    
    public static AffinityType GetPrioritizeAffinity(List<AffinityType> affinityTypes)
    {
        
        AffinityType[] affinityPriority =
        {
            AffinityType.Drain, AffinityType.Repel, AffinityType.Null, AffinityType.Weak, AffinityType.Neutral,
            AffinityType.Resist
        };
        
        foreach (var priority in affinityPriority)
        {
            if (affinityTypes.Contains(priority)) return priority;
        }
        return AffinityType.Neutral;
    }
    
    // Pointer
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
            AffinityType.Weak => WeakModifierAttack,
            AffinityType.Resist => ResistModifierAttack,
            AffinityType.Null => NullModifierAttack,
            AffinityType.Drain => DrainModifierAttack,
            _ => NeutralModifierAttack,
        };
    }

    public static int LightOrDarkAffinityModifier(Unit attacker, Unit target, Ability ability)
    {
        return target.Affinity.GetAffinity(ability.Type) switch
        {
            AffinityType.Weak => 1,
            AffinityType.Neutral => attacker.Attributes.Lck + ability.Power >= target.Attributes.Lck ? 1 : 0,
            AffinityType.Resist => attacker.Attributes.Lck + ability.Power >= 2 * target.Attributes.Lck ? 1 : 0,
            AffinityType.Null => 0,
            AffinityType.Repel => -1
        };
    }
    
}