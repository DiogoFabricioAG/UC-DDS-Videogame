using System.Diagnostics.CodeAnalysis;
using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_Model.Services;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController
{
    private View _view;
    private string _teamsFolder;
    private const string SEPARATOR = "----------------------------------------";
    private const string ERROR_MESSAGE = "Archivo de equipos inválido";
    private int _inputFromUser;
    private bool _executionRunning;
    private readonly TeamController _teamController;
    private readonly AttackService _attackService;
    private readonly AbilityService _abilityService;
    private int InputFromUser
    {
        get => _inputFromUser;
        set => _inputFromUser = value;
    }

    public GameController(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
        _teamController = new TeamController(_view);
        _attackService = new AttackService();
        _abilityService = new AbilityService();
    }
    
    private string TeamCreation(string[] lines, Team team1, Team team2)
    {
        var position1 = 0;
        int position2 = 1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Player 2"))
            {
                position2 = i;
                break;
            }
        }
        string[] alineacionE1 = lines.Skip(1).Take(position2-1).ToArray();
        string[] alineacionE2 = lines.Skip(position2+1).Take(lines.Length - position2 - 1).ToArray();

        var result = _teamController.EnterUnits(alineacionE1, team1);
        team1.Identifier = "1";
        team1.State = TeamState.WithTurn;
        if (result)
        {
            return ERROR_MESSAGE;
        }
        team1.SelectStarterTeam();
        team1.Samurai.ShowAbility();
        var result2 = _teamController.EnterUnits(alineacionE2, team2);
        
        team2.Identifier = "2";
        team2.State = TeamState.WithoutTurn;
        team2.Samurai.ShowAbility();

        if (result2)
        {
            return ERROR_MESSAGE;
        }
        team2.SelectStarterTeam();
        return SEPARATOR;
    }
    
    private void InputText(string text)
    {
        InputFromUser = Convert.ToInt32(text);
        _view.WriteLine(SEPARATOR);
    }
  private void HandleChangeTurn(Game game)
    {

        game.CurrentTeam.RealoadTurns();
        game.OtherTeam.RealoadTurns();
        _view.DisplayPlayerTurnExclamation(game.CurrentTeam);

        while (game.CurrentTeam.State == TeamState.WithTurn && game.HandleGameFinished() == null)
        {
            _view.DisplayTeamsUnitsCurrentStatus(game);
            _view.DisplayCurrentTurnsbyType(game.CurrentTeam);
            _view.DisplayCurrentTurnOrder(game.CurrentTeam);

            _executionRunning = true;

            while (_executionRunning)
            {
                var availableActions = game.CurrentTeam.WhoAttack().GetAvailableActions();
                _view.DisplayUnitActions(game.CurrentTeam.WhoAttack().Name, availableActions);

                InputText(_view.ReadLine());
                if (game.CurrentTeam.WhoAttack() is Monster)
                {
                    HandleActionUnit(game);
                }
                else
                {
                    HandleAction(game);
                }
                if (InputFromUser == 6)
                {
                    break;
                }

            }
            game.OtherTeam.AnyUnitDestroyed();

        }
        game.ChangeCurrentTeam();
    }
  
    private String LoadGame(Game game)
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

        _view.WriteLine($"Ganador: {game.HandleGameFinished().Name()}");
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
            
            case 5:
                HandlePassTurn(game);
                break;
            case 6:
                game.HandleSurrender();
                _view.SurrenderTeamDisplay(game.CurrentTeam);
                break;
        }
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
                break;
            case 4:
                HandlePassTurn(game);
                break;
        }
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
        
        var (attacker, attacked) = game.GetAttackerAndTarget(InputFromUser);
    
        var damageDone = _attackService.ExecuteAttack(attacker, attacked, elementType);

        _view.DisplayAttackLogs(damageDone, attacker, attacked, elementType);
    
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.DestroyTurn(TurnType.Full);
        game.CurrentTeam.TurnRemains();
        game.OtherTeam.WasDefeated();
    
        _view.TurnUsedDisplay();
    }

    private void HandlePassTurn(Game game)
    {
        var type =  game.PassTurn();
        if (type == TurnType.Full)
        {
            _view.TurnUsedDisplayWonBlink();
        }
        else
        {
            _view.BlinkTurnUsedDisplay();
        }
    }


    private void HandleAbilityUse(Game game)
    {
        _view.WriteLine($"Seleccione una habilidad para que {game.CurrentTeam.WhoAttack().Name} use");
        _view.DisplayShowSelectableAbilities(game.CurrentTeam.WhoAttack());
        InputText(_view.ReadLine());
        if (InputFromUser == game.CurrentTeam.GetCancelOptionAbilities())
        {
            _executionRunning = true;
            return;
        }
        
        var ability = game.CurrentTeam.WhoAttack().Abilities[InputFromUser-1];

        _view.DisplayShowSelectablesUnit(game.OtherTeam, game.CurrentTeam, ability.Target);
        InputText(_view.ReadLine());
        
        var (attacker, attacked) = game.GetAttackerAndTarget(InputFromUser);
        var (damageDone, affinityType) = AbilityService.UseDamageAbility(attacker, attacked, ability);
        
        _view.DisplayAbilityLogs(damageDone, attacker, attacked, affinityType);
        game.CurrentTeam.ChangeOrder();
        game.CurrentTeam.DestroyTurn(TurnType.Full);
        game.CurrentTeam.TurnRemains();
        game.OtherTeam.WasDefeated();
    }
    
}
