using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public abstract class AbilityController
{
    public static (int damageDone, AffinityType affinityType, int numberHits) UseDamageAbility(DamageContext ctx)
    {
        Ability.ValidateMp(ctx.User, ctx.Ability);
        var numberHits = MultiHitController.GetHits(ctx.Ability.Hits, ctx.Team.NumAbilitiesCast);
        var affinityType = ctx.Target.Affinity.KnowAffinity(ctx.Ability.Type);
        var modifier = Affinity.AffinityModifier(affinityType);

        var userDamage = ctx.Ability.Type switch
        {
            AbilityType.Phys => ctx.User.Attributes.StrikeDmg,
            AbilityType.Gun => ctx.User.Attributes.SkillDmg,
            _ => ctx.User.Attributes.MagicDmg
        };
        var damageDone = (int)(Math.Sqrt(ctx.Ability.Power * userDamage) * modifier);

        for (var i = 0; i < numberHits; i++)
        {
            if (affinityType == AffinityType.Repel)
                ctx.User.HandleDamage(damageDone);
            else
                ctx.Target.HandleDamage(damageDone);
        }
        ctx.User.Attributes.CurrentMp -= ctx.Ability.Cost;
        return (damageDone, affinityType, numberHits);
    }

    
    public static int UseHealAbility(Unit user, Unit selectedUnit, Ability ability)
    {
        user.ValidUseAbility(ability);
        var healRealized = Convert.ToInt32(ability.Power * selectedUnit.Attributes.MaxHp / 100);
        selectedUnit.HandleDamage(healRealized*-1);
        return healRealized;
    }
}