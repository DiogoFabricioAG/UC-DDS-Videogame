using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class CombatController(View view)
{
    private int _inputFromUser;
    private bool _executionRunning;
    private const string SEPARATOR = "----------------------------------------";

    public void HandleChangeTurn(Game game)
    {
        game.CurrentTeam.ReloadTurns(); 
        game.OtherTeam.ReloadTurns(); 
        
        game.CurrentTeam.TeamTurnOrder = 0;
        view.DisplayPlayerTurnExclamation(game.CurrentTeam);

        while (game.CurrentTeam.State == TeamState.WithTurn && !game.IsGameFinished())
        {
            DisplayTurnState(game);
            ProcessUnitActions(game); 
            DestroyUnitsInTurn(game);
        }
    
        game.ChangeCurrentTeam();
    }
    
    private void ProcessUnitActions(Game game)
    {
        _executionRunning = true;
    
        while (_executionRunning)
        {
            var unitInTurn = game.CurrentTeam.GetUnitInTurn();
        
            var availableActions = unitInTurn.GetAvailableActions();
            view.DisplayUnitActions(unitInTurn.Name, availableActions);

            InputText(view.ReadLine());

            HandleUnitAction(game);

            if (_inputFromUser == 6 && !_executionRunning)
            {
                break;
            }
        }
    }
    

    private void HandleUnitAction(Game game)
    {
        _executionRunning = false; 
        
        var unitInTurn = game.CurrentTeam.GetUnitInTurn();

        ExecuteActionCommand(game, unitInTurn);

        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }

    private void ExecuteActionCommand(Game game, Unit unitInTurn)
    {
        if (unitInTurn is Monster)
        {
            ExecutionActionCommandForMonster(game);
        }
        else 
        {
            ExecutionActionCommandForSamurai(game, unitInTurn);
        }
    }

    private void ExecutionActionCommandForMonster(Game game)
    {
        switch (_inputFromUser)
        {
            case 1:
                HandleAttackUse(game, ElementType.Physics);
                break;
            case 2: 
                HandleAbilityUse(game);
                break;
            case 3: 
                HandleInvokeUse(game);
                break;
            case 4: 
                HandlePassTurn(game);
                break;
        }
    }

    private void ExecutionActionCommandForSamurai(Game game, Unit unitInTurn)
    {

        switch (_inputFromUser)
        {
            case 1:
                HandleAttackUse(game, ElementType.Physics);
                break;
            case 2:
                HandleAttackUse(game, ElementType.Gun);
                break;
            case 3:
                HandleAbilityUse(game);
                break;
            case 4:
                HandleInvokeUse(game);
                break;
            case 5:
                HandlePassTurn(game);
                break;
            case 6:
                game.HandleSurrender();
                view.SurrenderTeamDisplay(game.CurrentTeam);
                break;
        }
    }

    private void HandlePassTurn(Game game)
    {
        var type =  game.PassTurn();
        var turnContext = type == TurnType.Full ? new TurnContext(0, 1, 1) : new TurnContext(1,0,0);
        view.TurnUsedDisplayWithParameters(turnContext);
    }
    
    
    private void InputText(string text)
    {
        _inputFromUser = Convert.ToInt32(text);
        view.WriteLine(SEPARATOR);
    }

    private void HandleAttackUse(Game game, ElementType elementType)
    {
        if (!TrySelectAndValidateTarget(game, out Unit attacked))
        {
            _executionRunning = true; 
            return;
        }

        ExecuteAndApplyBasicAttack(game, attacked, elementType);
    }


    private bool TrySelectAndValidateTarget(Game game, out Unit attacked)
    {
        attacked = null;

        view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, TargetType.Single);
        InputText(view.ReadLine());

        if (_inputFromUser == game.OtherTeam.CancelOptionInSelectableTeam())
        {
            return false;
        }
        
        Unit target = game.GetAttacked(_inputFromUser, game.OtherTeam);
        attacked = target;
        return true;
    }


    private void ExecuteAndApplyBasicAttack(Game game, Unit attacked, ElementType elementType)
    {
        var attacker = game.CurrentTeam.GetUnitInTurn();
        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
        
        var (damageDone, affinityType) = AttackController.ExecuteAttack(attacker, attacked, elementType);

        view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, abilityType, 1);
        var ctx = new TurnWastedContext(abilityType, attacked, game.CurrentTeam);
        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(ctx);
        var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.CheckTurnRemains();
        
        view.TurnUsedDisplayWithParameters(turnContext);
    }

    private void HandleAbilityUse(Game game)
    {
        var unitInTurn = game.CurrentTeam.GetUnitInTurn();
    
        view.WriteLine($"Seleccione una habilidad para que {unitInTurn.Name} use");

        view.DisplayShowSelectableAbilities(unitInTurn);
        InputText(view.ReadLine());

        if (_inputFromUser == game.CurrentTeam.GetCancelOptionAbilities())
        {
            _executionRunning = true;
            return;
        }
    
        var ability = unitInTurn.GetTotalAbilities()[_inputFromUser-1];

        if (ability.Effect.Contains("Summon"))
        {
            var deadUnitsToo = ability.Effect.Contains("dead");
            HandleInvokeUse(game, ability, deadUnitsToo);
        }
        else
        {
            ProcessTargetedAbility(game, ability);
        }
    }
    
    private void ProcessTargetedAbility(Game game, Ability ability)
    {
        var reviveAbility = ability.Effect.Contains("Revive");
        var needToShowSelectableUnits = false;
        
        if (ability.Target != TargetType.Party && ability.Target != TargetType.All && ability.Target != TargetType.Multi)
        {
            view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, ability.Target, reviveAbility);
            InputText(view.ReadLine());
            needToShowSelectableUnits = true;
        }
    
        var teamSelected = ability.Target == TargetType.Ally ? game.CurrentTeam : game.OtherTeam;
        if (needToShowSelectableUnits && ((!reviveAbility && _inputFromUser == teamSelected.CancelOptionInSelectableTeam()) || 
            (reviveAbility && _inputFromUser == game.CurrentTeam.GetCancelButtonReviveUnits())))
        {

      
            _executionRunning = true;
            return;
        }

        Unit attacker = game.GetAttacker();
        
        if (ability.Target is TargetType.Party or TargetType.All or TargetType.Multi)
        {
            var (containsMissAttacks, affintyTypes) = ApplyForAllAbilityEffect(game, ability, attacker);
            ApplyTurnCostAndCleanupInAll(game, ability, affintyTypes , containsMissAttacks);
        }

        else
        {
            Unit attacked = game.GetAttacked(_inputFromUser, teamSelected, reviveAbility);
            if (AbilityController.IsLightOrDark(ability))
            {
                DamageContext ctx = new DamageContext(attacker, attacked, ability,  game.CurrentTeam);
                var (state, affinityType) = AbilityController.UseAbilityLightOrDark(ctx);
                view.DisplayAbilityLightOrDark(attacker,  attacked, state ,ability.Type);
            }
            else
            {
                ApplyAbilityEffect(game, attacker, attacked, ability, reviveAbility);
            }
            ApplyTurnCostAndCleanup(game, attacked, ability);

        }
    
    }
    
    private void ApplyAbilityEffect(Game game, Unit attacker, Unit attacked, Ability ability, bool isReviveAbility)
    {
        var drainAbility = ability.Effect.Contains("drains");
        var isHpAbility = ability.Effect.Contains("HP");
        var isMpAbility = ability.Effect.Contains("MP");
        if (ability.Target != TargetType.Ally)
        {
            var contextDamage = new DamageContext(attacker, attacked, ability, game.CurrentTeam);
            if (!drainAbility)
            {
                BasicAbilityResponseSingle abilityResponse = AbilityController.GetBasicDataFromUseAnAbility(contextDamage);
                AbilityController.ApplyDamageToAnUnit(contextDamage, abilityResponse);
                view.DisplayAbilityLogs(abilityResponse.ValueAmount, attacker, attacked, 
                    abilityResponse.AffinityType, ability.Type, abilityResponse.NumberHits);
            }
            else
            {
                if (isHpAbility && !isMpAbility)
                {
                    DrainResponse drainResponse = AbilityController.DrainHPToOne(contextDamage);
                    attacker.BurnManaPoints(ability);
                    view.DisplayDrainAbilityLogs(drainResponse.Values, attacker, attacked, isHpAbility);
                }
                else if (isMpAbility && !isHpAbility)
                {
                    DrainResponse mpDrainResponse = AbilityController.DrainMPToOne(contextDamage);
                    
                    // Arregla esto porfavor
                    attacker.BurnManaPoints(ability);

                    if (attacker.Attributes.CurrentMp > attacker.Attributes.MaxMp)
                    {
                        attacker.Attributes.CurrentMp = attacker.Attributes.MaxMp;
                    }
                    view.DisplayDrainAbilityLogs(mpDrainResponse.Values, attacker, attacked, isHpAbility);

                }
                else 
                {
                    HpAndMpResponseSingle hpAndMpResponse = AbilityController.DrainStatsToOne(contextDamage);
                    
                    attacker.BurnManaPoints(ability);

                    if (attacker.Attributes.CurrentMp > attacker.Attributes.MaxMp)
                    {
                        attacker.Attributes.CurrentMp = attacker.Attributes.MaxMp;
                    }
                    view.DisplayDrainStatsAbilityLogs(hpAndMpResponse.HpDrain, hpAndMpResponse.MpDrain, attacker, attacked);
                }
            }
            
        }
        else
        {
            var healRealized = AbilityController.UseHealAbility(attacker, attacked, ability);

            if (isReviveAbility)
            {
                game.CurrentTeam.ReviveUnit(attacked, attacker); 
            }
            view.DisplayAbilityLogs(healRealized, attacker, attacked, AffinityType.Neutral, ability.Type, 1, isReviveAbility);
        }
    }

    private (bool containsMissAttacks, List<AffinityType> affintyTypes) ApplyForAllAbilityEffect(Game game, Ability ability, Unit attacker)
    {
        bool containMissAttacks = false;
        switch (ability.Target)
        {
            case TargetType.Party when !ability.Effect.Contains("exchange"):
            {
                QuantityDamageResponse healRealized = AbilityController.UseHealAbilityAllies(attacker, game.CurrentTeam, ability);
                view.DisplayAbilityAllLogs(healRealized.Values,  attacker, game.CurrentTeam, AffinityType.Neutral );
                break;
            }
            case TargetType.Party when ability.Effect.Contains("exchange"):
            {
                var actualUnitInOrder = game.GetAttacker();
                game.CurrentTeam.TeamTurnOrder--;
            
                QuantityDamageResponse healRealized = AbilityController.HealAndSacrifice(attacker, game.CurrentTeam, ability);
                
                view.DisplayAbilityAllLogs(healRealized.Values,  attacker, game.CurrentTeam, AffinityType.Neutral , sacrific: true);
                if (game.CurrentTeam.Samurai.Defeated)
                {
                    game.CurrentTeam.ReviveUnit(game.CurrentTeam.Samurai, actualUnitInOrder); 
                }
                break;
            }
            case TargetType.All when AbilityController.IsLightOrDark(ability):
            {
                LightOrDarkAbilityListResponse lightOrDarkAbilityResponse = AbilityController.UseAbilityLightOrDarkAll(game.GetAttacker() ,game.OtherTeam, ability);
                containMissAttacks = lightOrDarkAbilityResponse.States.Contains(LightOrDarkState.Miss);
                view.DisplayAbilityLightOrDarkAll(lightOrDarkAbilityResponse.States,  attacker, game.OtherTeam ,ability.Type);
                return (containMissAttacks, lightOrDarkAbilityResponse.Affinities);
            }
            case TargetType.All when ability.Effect.Contains("drains"):
            {
                var isHpAbility = ability.Effect.Contains("HP");
                var isMpAbility = ability.Effect.Contains("MP");
                
                if (isHpAbility && !isMpAbility)
                {
                    QuantityDamageResponse hpDrain = AbilityController.DrainHpAll(attacker, game.OtherTeam, ability);
                    view.DisplayDrainAllAbilityLogs(hpDrain.Values, attacker, game.OtherTeam, isHpAbility);
                }
                else if (isMpAbility && !isHpAbility)
                {
                    QuantityDamageResponse mpDrain = AbilityController.DrainMpAll(attacker, game.OtherTeam, ability);

                    if (attacker.Attributes.CurrentMp > attacker.Attributes.MaxMp)
                    {
                        attacker.Attributes.CurrentMp = attacker.Attributes.MaxMp;
                    }
                    view.DisplayDrainAllAbilityLogs(mpDrain.Values, attacker, game.OtherTeam, isHpAbility);
                }
                else 
                {
                    HpAndMpResponseList hpAndMpResponseList = AbilityController.DrainStatsAll(attacker, game.OtherTeam, ability);

                    if (attacker.Attributes.CurrentMp > attacker.Attributes.MaxMp)
                    {
                        attacker.Attributes.CurrentMp = attacker.Attributes.MaxMp;
                    }

                    view.DisplayDrainStatsAllAbilityLogs(hpAndMpResponseList.HpDrainTeam, hpAndMpResponseList.MpDrainTeam, attacker, game.OtherTeam);
                }

                break;
            }
            case TargetType.All:
            {
                BasicAbilityResponseList abilityResponseList = AbilityController.UseDamageAbilityAll(game.GetAttacker(), game.OtherTeam, ability);
                view.DisplayAbilityAttackAll(abilityResponseList.DamageDone, attacker, game.OtherTeam, abilityResponseList.AffinityTypes, ability.Type);
                return (containMissAttacks, abilityResponseList.AffinityTypes);
            }
            case TargetType.Multi:
            {
                BasicAbilityResponseList abilityResponseList = AbilityController.UseDamageMultiAll(game.GetAttacker(), game, ability);

                view.DisplayAbilityMultiAttack(abilityResponseList.DamageDone, attacker, game.OtherTeam, abilityResponseList.NumberHits,abilityResponseList.AffinityTypes, ability.Type);
                return (containMissAttacks, abilityResponseList.AffinityTypes);
            }
        }
        return (containMissAttacks, [AffinityType.Neutral]);
    }

    public object List { get; set; }

    private void ApplyTurnCostAndCleanup(Game game, Unit attacked, Ability ability)
    {
        var turnWastedCtx = new TurnWastedContext(ability.Type, attacked, game.CurrentTeam);
        var (blinkingLoss, fullLoss, blinkingWon) = TurnController.GetTurnWasted(turnWastedCtx);
        var turnContext = new TurnContext(blinkingLoss, fullLoss, blinkingWon);
        view.TurnUsedDisplayWithParameters(turnContext);

        game.CurrentTeam.AddAbilityNumberCast();
    
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        game.CurrentTeam.ChangeOrder(); 
        game.CurrentTeam.CheckTurnRemains();

        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }

    /// <summary>
    ///  Revisame por favor q esta to' feo, ese current Team dio mio
    /// </summary>
    private void ApplyTurnCostAndCleanupInAll(Game game, Ability ability, List<AffinityType> affinityTypes, bool containsMissAttacks)
    {
        
        var (blinkingLoss, fullLoss, blinkingWon) = TurnController.GetTurnWastedByTeam(ability, game.CurrentTeam,affinityTypes, containsMissAttacks);
        var turnContext = new TurnContext(blinkingLoss, fullLoss, blinkingWon);
        view.TurnUsedDisplayWithParameters(turnContext);

        game.CurrentTeam.AddAbilityNumberCast();
    
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        game.CurrentTeam.ChangeOrder(); 
        game.CurrentTeam.CheckTurnRemains();

        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }
    

    private void HandleInvokeUse(Game game, Ability ability = null, bool showAll = false)
    {

        view.ShowInvocableMonsters(game.CurrentTeam, showAll);
        InputText(view.ReadLine());

        if (_inputFromUser == game.CurrentTeam.GetCancelOptionInvoke(showAll))
        {
            _executionRunning = true;
            return;
        }

        var indexBackupUnit = _inputFromUser;
        var actualCurrentUnit = game.CurrentTeam.GetUnitInTurn();
        int indexStarterUnit;
        if (ability != null || actualCurrentUnit is Samurai )
        {
            view.ShowReplaceableUnits(game.CurrentTeam);
            InputText(view.ReadLine());
            
            if (_inputFromUser == game.CurrentTeam.GetCancelButtonReplacebleUnits())
            {
                _executionRunning = true;
                return;
            }
            indexStarterUnit = _inputFromUser;
            
        }
        else
        {
            indexStarterUnit = Array.IndexOf(game.CurrentTeam.StartingTeam,  actualCurrentUnit);
        }
        

        var (unitReplaced, wasRevived) = game.CurrentTeam.ReplaceUnit(indexBackupUnit, indexStarterUnit, showAll);

        if (wasRevived)
        {
            unitReplaced.ChangeStatus();
        }
        view.InvokeAnUnit(unitReplaced, wasRevived, actualCurrentUnit);


        if (ability != null)
        {
            actualCurrentUnit.Attributes.CurrentMp -= ability.Cost;
            game.CurrentTeam.AddAbilityNumberCast();

        }
        var turnWastedCtx = new TurnWastedContext(AbilityType.Special, unitReplaced, game.CurrentTeam, ability != null);
        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(turnWastedCtx);
        var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);

        view.TurnUsedDisplayWithParameters(turnContext);
        
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.CheckTurnRemains();
    }

    private static void DestroyUnitsInTurn(Game game)
    {
        TeamController.HandleUnitsDestroyed(game.CurrentTeam); 
        TeamController.HandleUnitsDestroyed(game.OtherTeam);
    }

    private static void HandleFinishGame(Game game)
    {
        game.OtherTeam.WasDefeated();
        game.CurrentTeam.WasDefeated();
    }
    
    private void DisplayTurnState(Game game)
    {
        view.DisplayTeamsUnitsCurrentStatus(game);
        view.DisplayCurrentTurnsbyType(game.CurrentTeam);
        view.DisplayCurrentTurnOrder(game.CurrentTeam);
    }

}