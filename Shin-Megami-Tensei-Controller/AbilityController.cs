using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public abstract class AbilityController
{
    public static (int damageDone, AffinityType affinityType, int numberHits) UseDamageAbility(DamageContext ctx)
    {
        var (damageDone, affinityType, numberHits) = CalculateAbilityDamage(ctx);
        var effectCtx =
            new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, damageDone, affinityType, numberHits);
        ApplyDamageAbilityEffect(effectCtx);

        return (damageDone, affinityType, numberHits);
    }

    private static (int damageDone, AffinityType affinityType, int numberHits) CalculateAbilityDamage(DamageContext ctx)
    {
        Ability.ValidateMp(ctx.User, ctx.Ability); 
        
        var numberHits = MultiHitController.GetHits(ctx.Ability.Hits, ctx.Team.NumAbilitiesCast);
        var affinityType = ctx.Target.Affinity.KnowAffinity(ctx.Ability.Type);
        var modifier = Affinity.AffinityModifier(affinityType);

        var userDamageStat = ctx.Ability.Type switch
        {
            AbilityType.Phys => ctx.User.Attributes.StrikeDmg,
            AbilityType.Gun => ctx.User.Attributes.SkillDmg,
            _ => ctx.User.Attributes.MagicDmg
        };
        
        var damageDone = (int)(Math.Sqrt(ctx.Ability.Power * userDamageStat) * modifier);
        
        return (damageDone, affinityType, numberHits);
    }
    
    private static void ApplyDamageAbilityEffect(AbilityEffectContext ctx)
    {
        for (var i = 0; i < ctx.numberHits; i++)
        {
            if (ctx.affinityType == AffinityType.Repel)
                ctx.user.HandleDamage(ctx.damageDone);
            else
                ctx.target.HandleDamage(ctx.damageDone);
        }
        
        ctx.user.Attributes.CurrentMp -= ctx.ability.Cost;
    }

    
    public static int UseHealAbility(Unit user, Unit selectedUnit, Ability ability)
    {
        var healRealized = CalculateHealAmount(user, selectedUnit, ability);
        
        ApplyHealEffect(selectedUnit, healRealized);
        
        return healRealized;
    }

    private static int CalculateHealAmount(Unit user, Unit selectedUnit, Ability ability)
    {
        user.ValidUseAbility(ability); 
        
        return Convert.ToInt32(ability.Power * selectedUnit.Attributes.MaxHp / 100);
    }
    
    private static void ApplyHealEffect(Unit selectedUnit, int healAmount)
    {
        selectedUnit.HandleDamage(healAmount * -1); 
    }
}