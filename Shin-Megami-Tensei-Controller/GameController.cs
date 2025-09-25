using System.Diagnostics.CodeAnalysis;
using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController
{
    private View _view;
    private string _teamsFolder;
    private const string SEPARATOR = "----------------------------------------";
    private const string ERROR_MESSAGE = "Archivo de equipos inválido";
    private int _inputFromUser;
    private bool wasCanceled = false;
    private readonly TeamController _teamController;
    public int InputFromUser
    {
        get => _inputFromUser;
        set => _inputFromUser = value;
    }

    public GameController(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
        _teamController = new TeamController(_view);
    }
    
    public string TeamCreation(string[] lines, Team team1, Team team2)
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
        var result2 = _teamController.EnterUnits(alineacionE2, team2);
        team2.Identifier = "2";
        team2.State = TeamState.WithoutTurn;
        Console.WriteLine("EQUIPO 2:"+ result2);

        if (result2)
        {
            return ERROR_MESSAGE;
        }
        team2.SelectStarterTeam();
        return SEPARATOR;
    }
    
    public void InputText(string text)
    {
        InputFromUser = Convert.ToInt32(text);
        _view.WriteLine(SEPARATOR);
    }
  public void HandleChangeTurn(Game game)
    {
        game.ChangeCurrentTeam();
        game.CurrentTeam.RealoadTurns();
        game.OtherTeam.RealoadTurns();
        _view.DisplayPlayerTurnExclamation(game.CurrentTeam);

        while (game.CurrentTeam.State == TeamState.WithTurn && game.CurrentTeam.GetCurrentFullTurns() != 0 && game.handleGameFinished() == null)
        {
            _view.DisplayTeamsUnitsCurrentStatus(game);
            _view.DisplayCurrentTurnsbyType(game.CurrentTeam);
            _view.DisplayCurrentTurnOrder(game.CurrentTeam);

            wasCanceled = true;

            while (wasCanceled)
            {
                var availableActions = game.CurrentTeam.WhoAttack().GetAvailableActions();
                _view.DisplayUnitActions(game.CurrentTeam.WhoAttack().Name, availableActions);

                InputText(_view.ReadLine());
                HandleAction(game);
                if (InputFromUser == 6)
                {
                    break;
                }

            }
            
            game.OtherTeam.AnyUnitDestroyed();
        }
        
    }
    
    public void Play()
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
        var seleccion = Convert.ToInt32(input);
        
        var archivoSeleccionado= Directory.GetFiles(_teamsFolder)[seleccion];
        var lines = File.ReadAllLines(archivoSeleccionado);

        var partida = new Shin_Megami_Tensei_Model.Game();
        var result = TeamCreation(lines, partida.Team1, partida.Team2);
        _view.WriteLine(result);
        if (result == "Archivo de equipos inválido")
        {
            return; 
        }
        partida.ChangeCurrentTeam();
        partida.ChangeCurrentTeam();
        while (partida.handleGameFinished() == null)
        {
            HandleChangeTurn(partida);
        }

        _view.WriteLine($"Ganador: {partida.handleGameFinished().Name()}");
    }

    private void HandleAction(Game game)
    {
        wasCanceled = false;
        switch (InputFromUser)
        {
            
            case 1:
                _view.WriteLine($"Seleccione un objetivo para {game.CurrentTeam.WhoAttack().Name}");
                _view.DisplayShowSelectablesUnit(game.OtherTeam);
                InputText(_view.ReadLine());
                if (InputFromUser == game.OtherTeam.GetNumberUnitsInStartingTeam() + 1)
                    wasCanceled = true;
                else
                {
                    Unit attacked = game.OtherTeam.GetSelectableUnits()[InputFromUser - 1];
                    int damageDone = game.CurrentTeam.WhoAttack()
                        .Attack(attacked, ElementType.Physics);
                    _view.DisplayAttackLogs(damageDone, game.CurrentTeam.WhoAttack(),
                        attacked, ElementType.Physics);
                    game.CurrentTeam.ChangeOrder();
                    game.CurrentTeam.DestroyTurn(TurnType.Full);
                    _view.TurnUsedDisplay();

                }
                
                break;

            case 2:
                _view.WriteLine($"Seleccione un objetivo para {game.CurrentTeam.WhoAttack().Name}");
                _view.DisplayShowSelectablesUnit(game.OtherTeam);
                InputText(_view.ReadLine());
                
                if (InputFromUser == game.OtherTeam.GetNumberUnitsInStartingTeam() + 1)
                    wasCanceled = true;
                
                else
                {
                    
                    Unit attacked = game.OtherTeam.GetSelectableUnits()[InputFromUser - 1];
                    int damageDone = game.CurrentTeam.WhoAttack()
                        .Attack(attacked, ElementType.Gun);
                    _view.DisplayAttackLogs(damageDone, game.CurrentTeam.WhoAttack(),
                        attacked, ElementType.Gun);
                    game.CurrentTeam.ChangeOrder();
                    
                    game.CurrentTeam.DestroyTurn(TurnType.Full);
                    _view.TurnUsedDisplay();

                }
                
                
                

                break;
            case 3:
                _view.WriteLine($"Seleccione una habilidad para que {game.CurrentTeam.WhoAttack().Name} use");
                _view.DisplayShowSelectableAbilities(game.CurrentTeam.WhoAttack());
                InputText(_view.ReadLine());
                Console.WriteLine("Numero de Habilidades: " + game.CurrentTeam.WhoAttack().GetTotalAbilities());
                if (InputFromUser == game.CurrentTeam.WhoAttack().GetTotalAbilities() + 1)
                {
                    wasCanceled = true;
                    game.CurrentTeam.ChangeOrder();
                }
                break;
            case 6:
                game.HandleSurrender();
                _view.SurrenderTeamDisplay(game.CurrentTeam);
                break;
        }
    }
}
