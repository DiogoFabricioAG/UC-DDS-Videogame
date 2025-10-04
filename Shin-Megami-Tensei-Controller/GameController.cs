using System.Diagnostics.CodeAnalysis;
using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController
{
    private View _view;
    private readonly string _teamsFolder;
    private const string SEPARATOR = "----------------------------------------";
    private const string ERROR_MESSAGE = "Archivo de equipos inválido";
    private bool _executionRunning;
    private readonly TeamController _teamController;

    private int InputFromUser { get; set; }

    public GameController(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
        _teamController = new TeamController(_view);

    }
    
    private string TeamCreation(string[] lines, Team team1, Team team2)
    {
        var alineations = ExtractTeamAlineations(lines);
        var team1Lines = alineations.team1Lines;
        var team2Lines = alineations.team2Lines;

        var result1 = ConfigureTeam(team1Lines, team1, "1", TeamState.WithTurn);
        if (result1 != SEPARATOR)
        {
            return result1; 
        }

        var result2 = ConfigureTeam(team2Lines, team2, "2", TeamState.WithoutTurn);
        if (result2 != SEPARATOR)
        {
            return result2; 
        }

        return SEPARATOR;
    }

    private (string[] team1Lines, string[] team2Lines) ExtractTeamAlineations(string[] lines)
    {
        var position2 = 1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Player 2"))
            {
                position2 = i;
                break;
            }
        }
        
        var team1Lines = lines.Skip(1).Take(position2 - 1).ToArray();
        var team2Lines = lines.Skip(position2 + 1).Take(lines.Length - position2 - 1).ToArray();

        return (team1Lines, team2Lines);
    }

    private string ConfigureTeam(string[] alineations, Team team, string identifier, TeamState initialState)
    {
        var hasError = _teamController.EnterUnits(alineations, team);
        
        if (hasError)
        {
            return ERROR_MESSAGE;
        }

        team.Identifier = identifier;
        team.State = initialState;
        TeamController.GenerateTeamForInitGame(team);
        
        team.Samurai.ShowAbility(); 

        return SEPARATOR; 
    }
    private void InputText(string text)
    {
        InputFromUser = Convert.ToInt32(text);
        _view.WriteLine(SEPARATOR);
    }

    private static void ReloadAllTurns(Game game)
    {
        game.CurrentTeam.ReloadTurns();
        game.OtherTeam.ReloadTurns();
    }
    private void HandleChangeTurn(Game game)
    {
        ReloadAllTurns(game);
        game.CurrentTeam.TeamTurnOrder = 0;
        _view.DisplayPlayerTurnExclamation(game.CurrentTeam);

        while (game.CurrentTeam.State == TeamState.WithTurn && game.HandleGameFinished() == null)
        {
            DisplayTurnState(game);
            ProcessUnitActions(game); 
            DestroyUnitsInTurn(game);
        }
    
        game.ChangeCurrentTeam();
    }

    
    private void DisplayTurnState(Game game)
    {
        _view.DisplayTeamsUnitsCurrentStatus(game);
        _view.DisplayCurrentTurnsbyType(game.CurrentTeam);
        _view.DisplayCurrentTurnOrder(game.CurrentTeam);
    }
    
    private void ProcessUnitActions(Game game)
    {
        _executionRunning = true;
    
        while (_executionRunning)
        {
            var unitInTurn = game.CurrentTeam.GetUnitInTurn();
        
            var availableActions = unitInTurn.GetAvailableActions();
            _view.DisplayUnitActions(unitInTurn.Name, availableActions);

            InputText(_view.ReadLine());

            if (unitInTurn is Monster)
            {
                HandleActionUnit(game); 
            }
            else
            {
                HandleAction(game);
            }

            if (InputFromUser == 6 && !_executionRunning)
            {
                break;
            }
        }
    }


  
    private string LoadGame(Game game)
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        if (Directory.Exists(_teamsFolder))
        {
            try
            {
                string[] archivos = Directory.GetFiles(_teamsFolder);
                int contador = 0;
                foreach (string archivo in archivos)
                {
                    _view.WriteLine($"{contador}: {Path.GetFileName(archivo)}");
                    contador++;
                }
                
            }
            catch (Exception ex)
            {
                _view.WriteLine($"Ocurrió un error al leer la carpeta: {ex.Message}");
            }
        }
        else
        {
            _view.WriteLine("La carpeta de equipos no existe.");
        }
        var input = _view.ReadLine();
        var selection = Convert.ToInt32(input);
        var selectedFile= Directory.GetFiles(_teamsFolder)[selection];
        var lines = File.ReadAllLines(selectedFile);
        var result = TeamCreation(lines, game.CurrentTeam, game.OtherTeam);
        return result;
    }
    public void Play()
    {
        var game = new Game();
        var resultLoad = LoadGame(game);
        _view.WriteLine(resultLoad);
        if (resultLoad == ERROR_MESSAGE)
        {
            return; 
        }
        RunGameLoop(game);
        _view.WriteLine($"Ganador: {game.HandleGameFinished().GetName()}");
    }

    private void RunGameLoop(Game game)
    {
        while (game.HandleGameFinished() == null)
        {
            HandleChangeTurn(game);
        }
    }

    private void HandleAction(Game game)
    {
        _executionRunning = false;
        switch (InputFromUser)
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
                _view.SurrenderTeamDisplay(game.CurrentTeam);
                break;
        }
        
        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }

    private void HandleActionUnit(Game game)
    {
        _executionRunning = false;
        switch (InputFromUser)
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

        DestroyUnitsInTurn(game);
        HandleFinishGame(game);
    }
    
    private void HandleAttackUse(Game game, ElementType elementType)
    {
        _view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, TargetType.Single);
        InputText(_view.ReadLine());
    
        if (InputFromUser == game.OtherTeam.CancelOptionInSelectableTeam())
        {
            _executionRunning = true;
            return;
        }
        
        var (attacker, attacked) = game.GetAttackerAndTarget(InputFromUser, game.OtherTeam);

        var (damageDone, affinityType) = AttackController.ExecuteAttack(attacker, attacked, elementType);

        
        var abilityType = elementType == ElementType.Physics ? AbilityType.Phys : AbilityType.Gun;
        _view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType, abilityType, 1);

        var (blinkingTurnLoss, fullTurnLoss, blinkingTurnWon) = TurnController.GetTurnWasted(abilityType, attacked, game.CurrentTeam);
        TurnController.DestroyAndAddTurns(game.CurrentTeam, blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.TurnRemains();
        
        _view.TurnUsedDisplayWithParameters(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
    }

    private void HandlePassTurn(Game game)
    {
        var type =  game.PassTurn();
        if (type == TurnType.Full)
        {
            _view.TurnUsedDisplayWithParameters(0,1,1);
        }
        else
        {
            _view.TurnUsedDisplayWithParameters(1,0,0);
        }
    }


    private void HandleAbilityUse(Game game)
    {
        _view.WriteLine($"Seleccione una habilidad para que {game.CurrentTeam.GetUnitInTurn().Name} use");
        _view.DisplayShowSelectableAbilities(game.CurrentTeam.GetUnitInTurn());
        InputText(_view.ReadLine());

        if (InputFromUser == game.CurrentTeam.GetCancelOptionAbilities())
        {
            _executionRunning = true;
            return;
        }
        
        var ability = game.CurrentTeam.GetUnitInTurn().GetTotalAbilities()[InputFromUser-1];


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
        
            
            if (InputFromUser == teamSelected.CancelOptionInSelectableTeam() || 
                reviveAbility && InputFromUser == game.CurrentTeam.GetCancelButtonReviveUnits())
            {
                _executionRunning = true;
                return;
            }

            var (attacker, attacked) = game.GetAttackerAndTarget(InputFromUser, teamSelected, reviveAbility);

            if (ability.Target != TargetType.Ally)
            {
                var (damageDone, affinityType, numberHits) = AbilityController.UseDamageAbility(attacker, attacked, ability, game.CurrentTeam);
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
                            Console.WriteLine("DESPUES");

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

        
            _view.TurnUsedDisplayWithParameters(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
            game.CurrentTeam.NumAbilitiesCast++;

            game.CurrentTeam.ChangeOrder();
        
        
            TurnController.DestroyAndAddTurns(game.CurrentTeam, blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
            
        
        
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

        if (InputFromUser == game.CurrentTeam.GetCancelOptionInvoke(showAll))
        {
            _executionRunning = true;
            return;
        }

        var indexBackupUnit = InputFromUser;
        var actualCurrentUnit = game.CurrentTeam.GetUnitInTurn();
        int indexStarterUnit;
        if (ability != null || actualCurrentUnit is Samurai )
        {
            _view.ShowReplaceableUnits(game.CurrentTeam);
            InputText(_view.ReadLine());
            
            if (InputFromUser == game.CurrentTeam.GetCancelButtonReplacebleUnits())
            {
                _executionRunning = true;
                return;
            }
            indexStarterUnit = InputFromUser;
            
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
        
        _view.TurnUsedDisplayWithParameters(blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        game.CurrentTeam.ChangeOrder();
        
        TurnController.DestroyAndAddTurns(game.CurrentTeam, blinkingTurnLoss, fullTurnLoss, blinkingTurnWon);
        game.CurrentTeam.TurnRemains();
    }
    private static void DestroyUnitsInTurn(Game game)
    {
        TeamController.HandleUnitsDestroyed(game.CurrentTeam);
        TeamController.HandleUnitsDestroyed(game.OtherTeam);
    }

    private void HandleFinishGame(Game game)
    {
        game.OtherTeam.WasDefeated();
        game.CurrentTeam.WasDefeated();
    }
}


