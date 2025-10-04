using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public abstract class AttackController
{
    private const double ATTACK_MULTIPLIER = 0.0114;
    private const int GUN_MODIFIER = 80;
    private const int PHYSICS_MODIFIER = 54;
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
        
        var modifier = elementType == ElementType.Physics ? PHYSICS_MODIFIER : GUN_MODIFIER;
        var statAttack = elementType == ElementType.Physics ? attacker.Attributes.StrikeDmg : attacker.Attributes.SkillDmg;
        var damageDone = (int)(modifier * statAttack * ATTACK_MULTIPLIER * affinityMofifier);
        if (affinityType == AffinityType.Repel)
        {
            attacker.HandleDamage(damageDone);
        }
        else
        {
            target.HandleDamage(damageDone);

        }
        
        return (damageDone, affinityType);
    }
}