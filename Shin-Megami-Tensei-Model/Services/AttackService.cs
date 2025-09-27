namespace Shin_Megami_Tensei_Model.Services;

public class AttackService
{
    private const double DamageModifier = 0.0114;
    private const int SkillModifier = 80;
    private const int PhysModifier = 54;
    public int ExecuteAttack(Unit attacker, Unit target, ElementType elementType)
    {
        var modifier = elementType == ElementType.Physics ? PhysModifier : SkillModifier;
        var statAttack = elementType == ElementType.Physics ? attacker.Attributes.StrikeDmg : attacker.Attributes.SkillDmg;
        var damageDone = (int)(modifier * statAttack * DamageModifier);

        target.TakeDamage(damageDone);
        
        return damageDone;
    }
}