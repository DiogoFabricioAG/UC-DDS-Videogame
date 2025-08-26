using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController
{
    private View _view;
    private string _teamsFolder;
    private const string SEPARATOR = "----------------------------------------";

    private int _inputFromUser;

    public int InputFromUser
    {
        get => _inputFromUser;
        set => _inputFromUser = value;
    }

    public GameController(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
    }

    public void LoadListValuesForView(string[] lines)
    {
        foreach (var line in lines)
        {
            _view.WriteLine(line);
        }
    }

    public void LoadSingleValueForView(string line)
    {
        _view.WriteLine(line);
    }

    public void InputText(string text)
    {
        InputFromUser = Convert.ToInt32(text);
        _view.WriteLine(SEPARATOR);
    }

    public void HandleChangeTurn(Game game)
    {
        game.ChangeCurrentTeam();
        game.currentTeam.RealoadTurns();
        game.otherTeam.RealoadTurns();
        LoadSingleValueForView(game.PlayerTurnExclamation(game.currentTeam));
        
        while (game.currentTeam.State == TeamState.WithTurn)
        {
            LoadListValuesForView(game.TeamsUnitsCurrentStatus());
            LoadListValuesForView(game.currentTeam.CurrentTurnsbyType());
            LoadSingleValueForView(SEPARATOR);
            LoadListValuesForView(game.currentTeam.CurrentTurnOrder());
            LoadSingleValueForView(SEPARATOR);
            LoadListValuesForView(game.currentTeam.WhoAttack().SelectOptions());
            InputText(_view.ReadLine());
            HandleAction(game);
            if (InputFromUser == 6)
            {
                LoadSingleValueForView(SEPARATOR);

                break;
            }
            
            game.otherTeam.AnyUnitDestroyed();
            LoadSingleValueForView(SEPARATOR);
            game.currentTeam.ChangeOrder();
            
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
        var result = partida.TeamCreation(lines);
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

        LoadSingleValueForView($"Ganador: {partida.handleGameFinished().Name()}");
    }

    private void HandleAction(Game partida)
    {
        switch (InputFromUser)
        {
            
            case 1:
                LoadSingleValueForView($"Seleccione un objetivo para {partida.currentTeam.WhoAttack().Name}");
                LoadListValuesForView(partida.otherTeam.ShowSelectablesUnit());
                InputText(_view.ReadLine());
                LoadListValuesForView(partida.AttackLogs(partida.currentTeam.WhoAttack()
                        .AttackKinds(partida.otherTeam.StartingTeam[InputFromUser - 1], ElementType.Physics),
                    partida.currentTeam.WhoAttack(), partida.otherTeam.StartingTeam[InputFromUser - 1], ElementType.Physics)); 
                LoadListValuesForView(partida.currentTeam.TurnUsed());
                break;

            case 2:
                LoadSingleValueForView($"Seleccione un objetivo para {partida.currentTeam.WhoAttack().Name}");
                LoadListValuesForView(partida.otherTeam.ShowSelectablesUnit());
                InputText(_view.ReadLine());
                LoadListValuesForView(partida.AttackLogs(partida.currentTeam.WhoAttack()
                        .AttackKinds(partida.otherTeam.StartingTeam[InputFromUser - 1], ElementType.Gun),
                    partida.currentTeam.WhoAttack(), partida.otherTeam.StartingTeam[InputFromUser - 1] , ElementType.Gun));
                LoadListValuesForView(partida.currentTeam.TurnUsed());

                break;
            case 6:
                partida.HandleSurrender();
                LoadSingleValueForView($"{partida.currentTeam.Name()} se rinde");
                break;
        }
    }
}
