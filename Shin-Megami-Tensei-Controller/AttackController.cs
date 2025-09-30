using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public class AttackController
{
    private const double DamageModifier = 0.0114;
    private const int SkillModifier = 80;
    private const int PhysModifier = 54;
    public static (int, AffinityType) ExecuteAttack(Unit attacker, Unit target, ElementType elementType )
    {

        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
        var affinityType = target.Affinity.KnowAffinity(abilityType);
        var affinityMofifier= 1.0;
        switch (affinityType)
        {
            case AffinityType.Weak:
                affinityMofifier = 1.5;
                break;
            case AffinityType.Resist:
                affinityMofifier = 0.5;
                break;
            case AffinityType.Null:
                affinityMofifier = 0;
                break;
            case AffinityType.Repel:
                break;
            case AffinityType.Drain:
                affinityMofifier = -1;
                break;
        }
        
        var modifier = elementType == ElementType.Physics ? PhysModifier : SkillModifier;
        var statAttack = elementType == ElementType.Physics ? attacker.Attributes.StrikeDmg : attacker.Attributes.SkillDmg;
        var damageDone = (int)(modifier * statAttack * DamageModifier * affinityMofifier);
        if (affinityType == AffinityType.Repel)
        {
            attacker.TakeDamage(damageDone);
        }
        else
        {
            target.TakeDamage(damageDone);

        }
        
        return (damageDone, affinityType);
    }
}