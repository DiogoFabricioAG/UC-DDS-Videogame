using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class CombatController
{
    private readonly View _view;
    private readonly TeamController _teamController;
    private int _inputFromUser; // Internal input state
    private bool _executionRunning; // Internal state for unit action loop
    private const string SEPARATOR = "----------------------------------------";
    public CombatController(View view, TeamController teamController)
    {
        _view = view;
        _teamController = teamController;
    }

    // HandleChangeTurn is the main combat loop orchestrator
    public void HandleChangeTurn(Game game)
    {
        // ReloadAllTurns is now in GameSetupService, or if turns reset *every* turn, 
        // it should be here, possibly renamed. Let's assume it should be here for turn-reset logic.
        game.CurrentTeam.ReloadTurns(); 
        game.OtherTeam.ReloadTurns(); 
        
        game.CurrentTeam.TeamTurnOrder = 0;
        _view.DisplayPlayerTurnExclamation(game.CurrentTeam);

        while (game.CurrentTeam.State == TeamState.WithTurn && !game.IsGameFinished())
        {
            DisplayTurnState(game);
            ProcessUnitActions(game); // The loop for I/O input
            DestroyUnitsInTurn(game);
        }
    
        game.ChangeCurrentTeam();
    }
    
    // ProcessUnitActions is moved here
    private void ProcessUnitActions(Game game)
    {
        _executionRunning = true;
        // ... (Original ProcessUnitActions logic here: while loop, DisplayUnitActions, ReadLine, HandleAction/HandleActionUnit)
        
        // This is where InputText logic needs to be integrated or delegated clearly.
        // Assuming we delegate InputText to the view and store the result.
        _inputFromUser = Convert.ToInt32(_view.ReadLine());
        _view.WriteLine("----------------------------------------");
        
        // ... (Original ProcessUnitActions switch logic)
    }
    
    // HandleAction and HandleActionUnit (Switches) are moved here
    private void HandleAction(Game game)
    {
        // ... (Original HandleAction logic, using _inputFromUser)
        // ... calls HandleAttackUse, HandleAbilityUse, etc.
        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }
    
    private void HandleActionUnit(Game game)
    {
        // ... (Original HandleActionUnit logic, using _inputFromUser)
        // ... calls HandleAttackUse, HandleAbilityUse, etc.
        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }
    
    private void InputText(string text)
    {
        _inputFromUser = Convert.ToInt32(text);
        _view.WriteLine(SEPARATOR);
    }

    // All Handle*Use methods are moved here
    private void HandleAttackUse(Game game, ElementType elementType)
    {
        _view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, TargetType.Single);
        InputText(_view.ReadLine());
    
        if (_inputFromUser == game.OtherTeam.CancelOptionInSelectableTeam())
        {
            _executionRunning = true;
            return;
        }
        
        var (attacker, attacked) = game.GetAttackerAndTarget(_inputFromUser, game.OtherTeam);

        var (damageDone, affinityType) = AttackController.ExecuteAttack(attacker, attacked, elementType);

        
        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
        _view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, abilityType, 1);

        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(abilityType, attacked, game.CurrentTeam);
        var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.TurnRemains();
        
        _view.TurnUsedDisplayWithParameters(turnContext);
    }
    private void HandleAbilityUse(Game game) 
    {
        _view.WriteLine($"Seleccione una habilidad para que {game.CurrentTeam.GetUnitInTurn().Name} use");
        _view.DisplayShowSelectableAbilities(game.CurrentTeam.GetUnitInTurn());
        InputText(_view.ReadLine());

        if (_inputFromUser == game.CurrentTeam.GetCancelOptionAbilities())
        {
            _executionRunning = true;
            return;
        }
        
        var ability = game.CurrentTeam.GetUnitInTurn().GetTotalAbilities()[_inputFromUser-1];


        var summonAbility = ability.Effect.Contains("Summon");
        
        if (summonAbility)
        {
            var deadUnitsToo = ability.Effect.Contains("dead");
            HandleInvokeUse(game, ability, deadUnitsToo);
        }
        else
        {
            
            // Alguna Forma de Validar esto de aca, digo que esta terriblemente feo.
            var reviveAbility = ability.Effect.Contains("Revive");
            
            _view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, ability.Target, reviveAbility);
            InputText(_view.ReadLine());
            
            
        
            var teamSelected = ability.Target == TargetType.Ally ? game.CurrentTeam : game.OtherTeam;
        
            
            if (_inputFromUser == teamSelected.CancelOptionInSelectableTeam() || 
                reviveAbility && _inputFromUser == game.CurrentTeam.GetCancelButtonReviveUnits())
            {
                _executionRunning = true;
                return;
            }

            var (attacker, attacked) = game.GetAttackerAndTarget(_inputFromUser, teamSelected, reviveAbility);

            if (ability.Target != TargetType.Ally)
            {
                var contextDamage = new DamageContext(attacker, attacked, ability, game.CurrentTeam);
                var (damageDone, affinityType, numberHits) = AbilityController.UseDamageAbility(contextDamage);
                _view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, ability.Type, numberHits);

            }
            else
            {

                var healRealized = AbilityController.UseHealAbility(attacker, attacked, ability);

                if (reviveAbility)
                {
                    game.CurrentTeam.DestroyedUnits.RemoveAt(game.CurrentTeam.DestroyedUnits.IndexOf(attacked));
                    
                    if (attacked is Samurai)
                    {
                        if (game.CurrentTeam.OrderForActions.IndexOf(attacked) == game.CurrentTeam.OrderForActions.Count)
                        {
                            game.CurrentTeam.OrderForActions.Add(attacked);
                        }
                        else
                        {
                            foreach (var unit in game.CurrentTeam.OrderForActions)
                            {
                                Console.WriteLine(unit.Name);
                            }
                            game.CurrentTeam.OrderForActions.Insert(game.CurrentTeam.OrderForActions.IndexOf(attacker) , attacked);
                            game.CurrentTeam.TeamTurnOrder++;
                        }
                    }
                }
                _view.DisplayAbilityLogs(healRealized, attacker, attacked, AffinityType.Neutral, ability.Type, 1, reviveAbility);
            }
        
            var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(ability.Type, attacked, game.CurrentTeam);

            var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);

            _view.TurnUsedDisplayWithParameters(turnContext);
            game.CurrentTeam.NumAbilitiesCast++;

            game.CurrentTeam.ChangeOrder();
        
        
            TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
            
        
        
            game.CurrentTeam.TurnRemains();
            DestroyUnitsInTurn(game);
            game.OtherTeam.WasDefeated();
            game.CurrentTeam.WasDefeated();
            

        }
    }

    private void HandleInvokeUse(Game game, Ability ability = null, bool showAll = false)
    {

        _view.ShowInvocableMonsters(game.CurrentTeam, showAll);
        InputText(_view.ReadLine());

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
            _view.ShowReplaceableUnits(game.CurrentTeam);
            InputText(_view.ReadLine());
            
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
        _view.InvokeAnUnit(unitReplaced, wasRevived, actualCurrentUnit);


        if (ability != null)
        {
            actualCurrentUnit.Attributes.CurrentMp -= ability.Cost;
            game.CurrentTeam.NumAbilitiesCast++;

        }
        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(AbilityType.Special, unitReplaced, game.CurrentTeam, ability != null);
        var turnContext = new TurnContext(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);

        _view.TurnUsedDisplayWithParameters(turnContext);
        game.CurrentTeam.ChangeOrder();
        
        TurnController.DestroyAndAddTurns(game.CurrentTeam, turnContext);
        game.CurrentTeam.TurnRemains();
    }

    private void DestroyUnitsInTurn(Game game)
    {
        // TeamController.HandleUnitsDestroyed should probably be team.CheckForDefeatedUnits()
        TeamController.HandleUnitsDestroyed(game.CurrentTeam); 
        TeamController.HandleUnitsDestroyed(game.OtherTeam);
    }

    private void HandleFinishGame(Game game)
    {
        game.OtherTeam.WasDefeated();
        game.CurrentTeam.WasDefeated();
    }
    
    private void DisplayTurnState(Game game)
    {
        _view.DisplayTeamsUnitsCurrentStatus(game);
        _view.DisplayCurrentTurnsbyType(game.CurrentTeam);
        _view.DisplayCurrentTurnOrder(game.CurrentTeam);
    }

}