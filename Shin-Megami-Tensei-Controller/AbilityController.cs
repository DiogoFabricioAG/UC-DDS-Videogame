using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public abstract class AbilityController
{
    public static void ApplyDamageToAnUnit(DamageContext ctx, BasicAbilityResponseSingle abilityResponseStructure)
    {
        var effectCtx =
            new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, abilityResponseStructure.ValueAmount, abilityResponseStructure.AffinityType, abilityResponseStructure.NumberHits);
        ApplyDamageAbilityEffect(effectCtx);
        ctx.User.BurnManaPoints(ctx.Ability);
    }
    public static bool IsLightOrDark(Ability ability) => ability.Type is AbilityType.Light or AbilityType.Dark;

    private static LightOrDarkAbilitySingleResponse UseDamageAbilityLightOrDark(DamageContext ctx)
    {
        var percentDamageDone = Affinity.LightOrDarkAffinityModifier(ctx.User, ctx.Target, ctx.Ability);
        var affinityType = ctx.Target.Affinity.GetAffinity(ctx.Ability.Type);
        var effectCtx = new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, percentDamageDone * ctx.Target.Attributes.MaxHp, affinityType, 1);
        ApplyDamageAbilityEffect(effectCtx);

        return percentDamageDone switch
        {
            1 => new LightOrDarkAbilitySingleResponse(LightOrDarkState.Kill, affinityType),
            0 => affinityType == AffinityType.Null ? new LightOrDarkAbilitySingleResponse(LightOrDarkState.Block, affinityType) : 
                new LightOrDarkAbilitySingleResponse(LightOrDarkState.Miss, affinityType),
            _ => new LightOrDarkAbilitySingleResponse(LightOrDarkState.Repel, affinityType)
        };
    }
    
    public static BasicAbilityResponseSingle GetBasicDataFromUseAnAbility(DamageContext ctx)
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
        BasicAbilityResponseSingle abilityResponseList = new BasicAbilityResponseSingle(damageDone, affinityType, numberHits);
        return abilityResponseList;
    }
    
    private static void ApplyDamageAbilityEffect(AbilityEffectContext ctx, bool calculateAlready = false)
    {
        if (!calculateAlready)
        {
            for (var i = 0; i < ctx.numberHits; i++)
            {
                if (ctx.affinityType == AffinityType.Repel)
                    ctx.user.HandleDamage(ctx.damageDone);
                else
                    ctx.target.HandleDamage(ctx.damageDone);
            }
        }
        else
        {
            if (ctx.affinityType == AffinityType.Repel)
                ctx.user.HandleDamage(ctx.damageDone);
            else
                ctx.target.HandleDamage(ctx.damageDone);
        }
    }

    public static DrainResponse DrainHPToOne(DamageContext ctx)
    {
        BasicAbilityResponseSingle abilityResponseStructure = GetBasicDataFromUseAnAbility(ctx);
        abilityResponseStructure.ValueAmount = ctx.Target.AllHpReturn(abilityResponseStructure.ValueAmount);
        AbilityEffectContext abilityCtx = new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, abilityResponseStructure.ValueAmount, 
            abilityResponseStructure.AffinityType,abilityResponseStructure.NumberHits);
        ApplyDamageAbilityEffect(abilityCtx, true);
        
        ctx.User.HandleDamage(-abilityResponseStructure.ValueAmount);
        DrainResponse drainResponse = new DrainResponse(abilityResponseStructure.ValueAmount, abilityResponseStructure.AffinityType);
        return drainResponse;
    }
    
    public static DrainResponse DrainMPToOne(DamageContext ctx)
    {
        BasicAbilityResponseSingle abilityResponseStructure= GetBasicDataFromUseAnAbility(ctx);
        abilityResponseStructure.ValueAmount = ctx.Target.AllMpReturn(abilityResponseStructure.ValueAmount);
        ctx.Target.Attributes.CurrentMp -= abilityResponseStructure.ValueAmount;        
        ctx.User.Attributes.CurrentMp += abilityResponseStructure.ValueAmount;
        DrainResponse mpDrainResponse = new DrainResponse(abilityResponseStructure.ValueAmount, abilityResponseStructure.AffinityType);
        return mpDrainResponse;
    }

    public static HpAndMpResponseSingle DrainStatsToOne(DamageContext ctx)
    {
        BasicAbilityResponseSingle abilityResponseStructure = GetBasicDataFromUseAnAbility(ctx);
        int mpDrain = ctx.Target.AllMpReturn(abilityResponseStructure.ValueAmount);
        int hpDrain = ctx.Target.AllHpReturn(abilityResponseStructure.ValueAmount);
        ctx.Target.Attributes.CurrentMp -= mpDrain;        
        ctx.User.Attributes.CurrentMp += mpDrain;
        AbilityEffectContext abilityCtx = new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, hpDrain, abilityResponseStructure.AffinityType,abilityResponseStructure.NumberHits);
        ApplyDamageAbilityEffect(abilityCtx, true);
        
        ctx.User.HandleDamage(-hpDrain);
        HpAndMpResponseSingle hpAndMpResponse =  new HpAndMpResponseSingle(hpDrain, mpDrain);
        return hpAndMpResponse;
    }

    public static HpAndMpResponseList DrainStatsAll(Unit user, Team team, Ability ability)
    {
        List<int> hpDrainTeam =  new List<int>();
        List<int> mpDrainTeam =  new List<int>();
        user.BurnManaPoints(ability);

        foreach (var target in team.GetUnitsStillAlive())
        {
            DamageContext ctx = new DamageContext(user,target,ability, team);
            HpAndMpResponseSingle hpAndMpResponse = DrainStatsToOne(ctx);
            hpDrainTeam.Add(hpAndMpResponse.HpDrain);
            mpDrainTeam.Add(hpAndMpResponse.MpDrain);
        }
        HpAndMpResponseList hpAndMpResponseList = new HpAndMpResponseList(hpDrainTeam, mpDrainTeam);
        return hpAndMpResponseList;
    }

    public static QuantityDamageResponse DrainHpAll(Unit user, Team team,
        Ability ability)
    {   
        List<int> damageDone = new List<int>();
        List<AffinityType> affinityTypes = new List<AffinityType>();

        foreach (var target in team.GetUnitsStillAlive())
        {
            DamageContext ctx = new DamageContext(user, target, ability, team);
            DrainResponse drainResponse = DrainHPToOne(ctx);
            damageDone.Add(drainResponse.Values);
            affinityTypes.Add(drainResponse.AffinityType);
        }
        user.BurnManaPoints(ability);
        QuantityDamageResponse damageResponse = new QuantityDamageResponse(damageDone);
        return damageResponse;
    }
    
    public static QuantityDamageResponse DrainMpAll(Unit user, Team team,
        Ability ability)
    {   
        List<int> damageDone = new List<int>();
        List<AffinityType> affinityTypes = new List<AffinityType>();
        user.BurnManaPoints(ability);

        foreach (var target in team.GetUnitsStillAlive())
        {
            DamageContext ctx = new DamageContext(user, target, ability, team);
            DrainResponse drainResponse = DrainMPToOne(ctx);
            damageDone.Add(drainResponse.Values);
            affinityTypes.Add(drainResponse.AffinityType);
        }
        user.BurnManaPoints(ability);
        QuantityDamageResponse mpResponse = new QuantityDamageResponse(damageDone);
        return mpResponse;
    }
    
    

    public static QuantityDamageResponse HealAndSacrifice(Unit user, Team team,Ability ability)
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

    public static QuantityDamageResponse UseHealAbilityAllies(Unit user, Team team,Ability ability, bool revive = false)
    {
        List<int> healAllies = [];
        foreach (var unit in revive ? team.AllUnitsExceptInTurn(user): team.HelperSelectableUnitsForHealAbilities(user) )
        {
            var healRealized = CalculateHealAmount(user, unit, ability);
            
            ApplyHealEffect(unit, healRealized);
            
            healAllies.Add(healRealized);
        }                

        user.BurnManaPoints(ability);
        QuantityDamageResponse healResponse = new QuantityDamageResponse(healAllies);
        return healResponse;
    }

    public static BasicAbilityResponseList UseDamageAbilityAll(Unit user, Team team,Ability ability)
    {
        List<int> damageDoneForAll = new List<int>();
        List<AffinityType> affinityTypes = new List<AffinityType>();
        foreach (Unit target in team.GetUnitsStillAlive())
        {
            var ctx = new DamageContext(user, target, ability, team);
            BasicAbilityResponseSingle abilityResponseStructure = GetBasicDataFromUseAnAbility(ctx);
            var effectCtx =
                new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, abilityResponseStructure.ValueAmount,
                    abilityResponseStructure.AffinityType, abilityResponseStructure.NumberHits);
            ApplyDamageAbilityEffect(effectCtx);
            damageDoneForAll.Add(abilityResponseStructure.ValueAmount);
            affinityTypes.Add(abilityResponseStructure.AffinityType);
        }
        user.BurnManaPoints(ability);
        BasicAbilityResponseList abilityResponseList = new BasicAbilityResponseList(damageDoneForAll, affinityTypes);
        return abilityResponseList;
    }

    public static BasicAbilityResponseList UseDamageMultiAll(Unit user, Game game,
        Ability ability)
    {
        List<int> damageDoneForAll = new List<int>();
        List<AffinityType> affinityTypes = new List<AffinityType>();

        int pointer = 0;
        int numHits = MultiHitController.GetHits(ability.Hits, game.CurrentTeam.NumAbilitiesCast);
        List<int> numHitsForAll =
            MultiHitController.GetHitsForAttackers(numHits, game.OtherTeam, game.CurrentTeam.NumAbilitiesCast);
        
        foreach (Unit target in game.OtherTeam.GetUnitsStillAlive())
        {
            var ctx = new DamageContext(user, target, ability, game.CurrentTeam);
            BasicAbilityResponseSingle abilityResponseStructure = GetBasicDataFromUseAnAbility(ctx);
            var effectCtx =
                new AbilityEffectContext(ctx.User, ctx.Target, ctx.Ability, abilityResponseStructure.ValueAmount, 
                    abilityResponseStructure.AffinityType, abilityResponseStructure.NumberHits);
            for (var i = 0; i < numHitsForAll[pointer]; i++)
            {
                ApplyDamageAbilityEffect(effectCtx, true);

            }
            damageDoneForAll.Add(abilityResponseStructure.ValueAmount);
            affinityTypes.Add(numHitsForAll[pointer] == 0 ? AffinityType.Neutral : abilityResponseStructure.AffinityType);
            pointer++;
        }
        user.BurnManaPoints(ability);
        BasicAbilityResponseList abilityResponseList = new BasicAbilityResponseList(damageDoneForAll, affinityTypes, numHitsForAll);
        return abilityResponseList;
    }

    public static (LightOrDarkState state, AffinityType affinityType) UseAbilityLightOrDark(DamageContext ctx)
    {
        LightOrDarkAbilitySingleResponse lightOrDarkAbility = UseDamageAbilityLightOrDark(ctx);
        ctx.User.BurnManaPoints(ctx.Ability);

        return (lightOrDarkAbility.State, lightOrDarkAbility.Affinity);
    }
    public static LightOrDarkAbilityListResponse UseAbilityLightOrDarkAll(Unit user, 
        Team team, Ability ability)
    {
        List<LightOrDarkState> lightOrDarkStates = new List<LightOrDarkState>();
        List<AffinityType> affinities = new List<AffinityType>();
        
        foreach (Unit target in team.GetSelectableUnitsForAllAttacks())
        {
            if (target.Attributes.CurrentHp > 0)
            {
                var ctx = new DamageContext(user, target, ability, team);
                LightOrDarkAbilitySingleResponse lightOrDarkAbility= UseDamageAbilityLightOrDark(ctx);
                
                
                lightOrDarkStates.Add(lightOrDarkAbility.State);
                affinities.Add(lightOrDarkAbility.Affinity);
            }
            else
            {
                lightOrDarkStates.Add(LightOrDarkState.Empty);
            }
        }
        user.BurnManaPoints(ability);
        return new LightOrDarkAbilityListResponse(lightOrDarkStates, affinities);
    }

    private static int CalculateHealAmount(Unit user, Unit selectedUnit, Ability ability)
    {
        user.TryUseSkill(ability); 
        
        return Convert.ToInt32(ability.Power * selectedUnit.Attributes.MaxHp / 100);
    }
    
    private static void ApplyHealEffect(Unit selectedUnit, int healAmount)
    {
        selectedUnit.HandleDamage(healAmount * -1); 
    }
}