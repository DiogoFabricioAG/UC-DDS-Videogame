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
        
        var (_, target) = game.GetAttackerAndTarget(_inputFromUser, game.OtherTeam);
        
        attacked = target;
        return true;
    }


    private void ExecuteAndApplyBasicAttack(Game game, Unit attacked, ElementType elementType)
    {
        var attacker = game.CurrentTeam.GetUnitInTurn();
        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
        
        var (damageDone, affinityType) = AttackController.ExecuteAttack(attacker, attacked, elementType);

        view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, abilityType, 1);

        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(abilityType, attacked, game.CurrentTeam);
        var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.TurnRemains();
        
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
    
        view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, ability.Target, reviveAbility);
        InputText(view.ReadLine());
    
        var teamSelected = ability.Target == TargetType.Ally ? game.CurrentTeam : game.OtherTeam;
    
        if (_inputFromUser == teamSelected.CancelOptionInSelectableTeam() || 
            (reviveAbility && _inputFromUser == game.CurrentTeam.GetCancelButtonReviveUnits()))
        {
            _executionRunning = true;
            return;
        }

        var (attacker, attacked) = game.GetAttackerAndTarget(_inputFromUser, teamSelected, reviveAbility);
    
        ApplyAbilityEffect(game, attacker, attacked, ability, reviveAbility);
    
        ApplyTurnCostAndCleanup(game, attacked, ability);
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
    
    private void ApplyTurnCostAndCleanup(Game game, Unit attacked, Ability ability)
    {
        var (blinkingLoss, fullLoss, blinkingWon) = TurnController.GetTurnWasted(ability.Type, attacked, game.CurrentTeam);
        var turnContext = new TurnContext(blinkingLoss, fullLoss, blinkingWon);
        view.TurnUsedDisplayWithParameters(turnContext);

        game.CurrentTeam.NumAbilitiesCast++;
        game.CurrentTeam.ChangeOrder(); 
    
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        game.CurrentTeam.TurnRemains();

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
            game.CurrentTeam.NumAbilitiesCast++;

        }
        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(AbilityType.Special, unitReplaced, game.CurrentTeam, ability != null);
        var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);

        view.TurnUsedDisplayWithParameters(turnContext);
        game.CurrentTeam.ChangeOrder();
        
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        game.CurrentTeam.TurnRemains();
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