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
        else 
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
    
        var teamSelected = ability.Target == TargetType.Ally ? game.CurrentTeam : game.OtherTeam; // Revisame esto pofavo
        if (needToShowSelectableUnits && (_inputFromUser == teamSelected.CancelOptionInSelectableTeam() || 
            (reviveAbility && _inputFromUser == game.CurrentTeam.GetCancelButtonReviveUnits())))
        {
            _executionRunning = true;
            return;
        }

        Unit attacker = game.GetAttacker();
        
        if (ability.Target is TargetType.Party or TargetType.All or TargetType.Multi)
        {
            Team applyTo = ability.Target == TargetType.Party ? game.CurrentTeam : game.OtherTeam; 
            var (containsMissAttacks, affintyTypes) = ApplyForAllAbilityEffect(game, ability, attacker);
            ApplyTurnCostAndCleanupInAll(game, ability, affintyTypes , containsMissAttacks);
        }

        else
        {
            Unit attacked = game.GetAttacked(_inputFromUser, teamSelected, reviveAbility);

            ApplyAbilityEffect(game, attacker, attacked, ability, reviveAbility);
            ApplyTurnCostAndCleanup(game, attacked, ability);

        }
    
    }
    
    private void ApplyAbilityEffect(Game game, Unit attacker, Unit attacked, Ability ability, bool isReviveAbility)
    {
        if (ability.Target != TargetType.Ally)
        {
            var contextDamage = new DamageContext(attacker, attacked, ability, game.CurrentTeam);
            var (damageDone, affinityType, numberHits) = AbilityController.UseDamageAbility(contextDamage);
            view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, ability.Type, numberHits);
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
        if (ability.Target == TargetType.Party && !ability.Effect.Contains("exchange"))
        {
            List<int> healRealized = AbilityController.UseHealAbilityAllies(attacker, game.CurrentTeam, ability);
            view.DisplayAbilityAllLogs(healRealized,  attacker, game.CurrentTeam, AffinityType.Neutral ,ability.Type);
        }
        else if (ability.Target == TargetType.Party && ability.Effect.Contains("exchange"))
        {
            
            // Arreglame esto porfavor 
            game.CurrentTeam.TeamTurnOrder--;
            List<int> healRealized = AbilityController.HealAndSacrifice(attacker, game.CurrentTeam, ability);

            view.DisplayAbilityAllLogs(healRealized,  attacker, game.CurrentTeam, AffinityType.Neutral ,ability.Type ,sacrific: true);
        }
        else if (ability.Target == TargetType.All)
        {
            if (AbilityController.IsLightOrDark(ability))
            {
                var (states, affinityTypes) = AbilityController.UseAbilityLightOrDarkAll(game.GetAttacker() ,game.OtherTeam, ability);
                containMissAttacks = states.Contains(LightOrDarkState.Miss);
                view.DisplayAbilityLightOrDarkAll(states,  attacker, game.OtherTeam ,ability.Type);
                return (containMissAttacks, affinityTypes);
            }
            else
            {
                var (allDamageDone, affinityTypes) = AbilityController.UseDamageAbilityAll(game.GetAttacker(), game.OtherTeam, ability);
                view.DisplayAbilityAttackAll(allDamageDone, attacker, game.OtherTeam, affinityTypes, ability.Type);
                return (containMissAttacks, affinityTypes);
            }
        }
        else if (ability.Target == TargetType.Multi)
        {
            var (allDamageDone, affinityTypes, numHits) = AbilityController.UseDamageMultiAll(game.GetAttacker(), game, ability);
            foreach (var VARIABLE in affinityTypes)
            {
                Console.WriteLine("VARIABLE: " + VARIABLE );
            }
            view.DisplayAbilityMultiAttack(allDamageDone, attacker, game.OtherTeam, numHits,affinityTypes, ability.Type);
            return (containMissAttacks, affinityTypes);
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