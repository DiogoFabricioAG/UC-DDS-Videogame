using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public abstract class AttackController
{
    private const double ATTACK_MULTIPLIER = 0.0114;
    private const int GUN_MODIFIER = 80;
    private const int PHYSICS_MODIFIER = 54;
    

    private static (int damage, AffinityType affinity) CalculateDamage(Unit attacker, Unit target, ElementType elementType)
    {
        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
    
        var affinityType = target.Affinity.GetAffinity(abilityType);
        var affinityModifier = Affinity.AffinityModifier(affinityType);
    
        var baseModifier = elementType == ElementType.Physics ? PHYSICS_MODIFIER : GUN_MODIFIER;
        var statAttack = elementType == ElementType.Physics ? attacker.Attributes.StrikeDmg : attacker.Attributes.SkillDmg;
    
        var damageDone = (int)(baseModifier * statAttack * ATTACK_MULTIPLIER * affinityModifier);
    
        return (damageDone, affinityType);
    }

    public static (int damage, AffinityType affinity) ExecuteAttack(Unit attacker, Unit target, ElementType elementType)
    {
        var (damageDone, affinityType) = CalculateDamage(attacker, target, elementType);

        if (affinityType == AffinityType.Repel)
        {
            attacker.HandleDamage(damageDone);
        }else
        {
            target.HandleDamage(damageDone);
        }
    
        return (damageDone, affinityType);
    }
}