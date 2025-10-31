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
        ctx.User.Attributes.CurrentMp -= ctx.Ability.Cost;

        return (damageDone, affinityType, numberHits);
    }
    public static bool IsLightOrDark(Ability ability) => ability.Type is AbilityType.Light or AbilityType.Dark;

    private static LightOrDarkState UseDamageAbilityLightOrDark(DamageContext ctx)
    {
        var percentDamageDone = Affinity.LightOrDarkAffinityModifier(ctx.User, ctx.Target, ctx.Ability);
        var affinityType = ctx.Target.Affinity.GetAffinity(ctx.Ability.Type);
        var effectCtx = new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, percentDamageDone * ctx.Target.Attributes.MaxHp, affinityType, 1);
        ApplyDamageAbilityEffect(effectCtx);

        return percentDamageDone switch
        {
            1 => LightOrDarkState.Kill,
            0 => affinityType == AffinityType.Null ? LightOrDarkState.Block : LightOrDarkState.Miss,
            _ => LightOrDarkState.Repel
        };
    }
    private static (int damageDone, AffinityType affinityType, int numberHits) CalculateAbilityDamage(DamageContext ctx)
    {
        Ability.ValidateMp(ctx.User, ctx.Ability); 
        
        var numberHits = MultiHitController.GetHits(ctx.Ability.Hits, ctx.Team.NumAbilitiesCast);
        var affinityType = ctx.Target.Affinity.GetAffinity(ctx.Ability.Type);
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
    }

    public static List<int> HealAndSacrifice(Unit user, Team team,Ability ability)
    {
        user.HandleDamage(user.Attributes.CurrentHp);
        
        return UseHealAbilityAllies(user, team, ability, true);
    }

    
    public static int UseHealAbility(Unit user, Unit selectedUnit, Ability ability)
    {
        var healRealized = CalculateHealAmount(user, selectedUnit, ability);
        user.BurnManaPoints(ability);
        
        ApplyHealEffect(selectedUnit, healRealized);
        
        return healRealized;
    }

    public static List<int> UseHealAbilityAllies(Unit user, Team team,Ability ability, bool revive = false)
    {
        List<int> healAllies = [];
        foreach (var unit in revive ? team.AllUnitsExceptInTurn(user): team.HelperSelectableUnitsForHealAbilities(user) )
        {
            var healRealized = CalculateHealAmount(user, unit, ability);

            ApplyHealEffect(unit, healRealized);

            healAllies.Add(healRealized);
        }
        user.BurnManaPoints(ability);

        return healAllies;
    }

    public static List<LightOrDarkState> UseAbilityLightOrDarkAll(Unit user, Team team, Ability ability)
    {
        List<LightOrDarkState> lightOrDarkStates = new List<LightOrDarkState>();
        
        foreach (var target in team.GetSelectableUnitsForLightAndDark())
        {
            if (target.Attributes.CurrentHp > 0)
            {
                var ctx = new DamageContext(user, target, ability, team);
                var state = UseDamageAbilityLightOrDark(ctx);
                
                // Pointer 
                
                lightOrDarkStates.Add(state);
            }
            else
            {
                lightOrDarkStates.Add(LightOrDarkState.Empty);
            }
        }
        user.Attributes.CurrentMp -= ability.Cost;
        return lightOrDarkStates;
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