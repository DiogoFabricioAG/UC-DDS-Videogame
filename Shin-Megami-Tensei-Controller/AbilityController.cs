using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public class AbilityController
{
    public static (int, AffinityType, int numberHits) UseDamageAbility(Unit user,Unit selectedUnit ,Ability ability, Team team)
    {
        if (user.Attributes.CurrentMp < ability.Cost)
        {
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
        }

        var numberHits = 1;
        if (ability.Hits.Contains('-'))
        {
            var lowerRange = Convert.ToInt32(ability.Hits.Split('-')[0]);
            var upperRange = Convert.ToInt32(ability.Hits.Split('-')[1]);
            numberHits = MultiHitController.HandleMultiHit(team.NumAbilitiesCast, lowerRange, upperRange);
        }

        double affinityMofifier = 1;
        var affinityType = selectedUnit.Affinity.KnowAffinity(ability.Type);
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
        var userDamageByType = ability.Type == AbilityType.Phys ? user.Attributes.StrikeDmg : 
            ability.Type == AbilityType.Gun ? user.Attributes.SkillDmg : 
            user.Attributes.MagicDmg;
        var damageDone = (int)(Math.Sqrt(ability.Power * userDamageByType) * affinityMofifier);
        for (var i = 0; i < numberHits; i++)
        {
            if (affinityType == AffinityType.Repel)
            {
                user.HandleDamage(damageDone);
            }
            else
            {
                selectedUnit.HandleDamage(damageDone);
            }
        }
        
        user.Attributes.CurrentMp -= ability.Cost;
        team.NumAbilitiesCast++;
        return (damageDone,affinityType, numberHits);
    }

    
    // Aun no se utiliza
    public static int UseHealAbility(Unit user, Unit selectedUnit, Ability ability)
    {
        if (user.Attributes.CurrentMp < ability.Cost)
        {
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
        }

        var healRealized = Convert.ToInt32(ability.Power * selectedUnit.Attributes.MaxHp / 100);
        
        selectedUnit.HandleDamage(healRealized*-1); 
        user.Attributes.CurrentMp -= ability.Cost;
        return healRealized;
    }
}