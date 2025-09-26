namespace Shin_Megami_Tensei_Model.Services;

public class AttackService
{
    public int ExecuteAttack(Unit attacker, Unit target, ElementType elementType)
    {
        var modifier = elementType == ElementType.Physics ? 54 : 80;
        var statAttack = elementType == ElementType.Physics ? attacker.Attributes.StrikeDmg : attacker.Attributes.SkillDmg;
        var damageDone = (int)(modifier * statAttack * 0.0114);

        target.TakeDamage(damageDone);
        
        return damageDone;
    }
}