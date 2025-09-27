using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model.Services;

public class AbilityService
{
    public static (int, AffinityType) UseDamageAbility(Unit user,Unit selectedUnit ,Ability ability)
    {
        if (user.Attributes.CurrentMp < ability.Cost)
        {
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
        }

        double affinityMofifier = 1;
        var affinityType = selectedUnit.Affinity.KnowAffinity(ability);
        
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
        
        var userDamageByType = ability.Type == AbilityType.Phys ? user.Attributes.StrikeDmg : user.Attributes.MagicDmg;
        var damageDone = (int)(Math.Sqrt(ability.Power * userDamageByType) * affinityMofifier);
        selectedUnit.Attributes.CurrentHp -= damageDone;
        user.Attributes.CurrentMp -= ability.Cost;

        return (damageDone,affinityType);
    }

    public void HealAbility(Unit user, Unit selectedUnit, Ability ability)
    {
        if (user.Attributes.CurrentMp < ability.Cost)
        {
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
        }
        
        var healPercentage = ability.Power/100;
        user.Attributes.CurrentMp -= ability.Cost;
        selectedUnit.Attributes.CurrentHp += healPercentage*selectedUnit.Attributes.CurrentHp;
    }
}