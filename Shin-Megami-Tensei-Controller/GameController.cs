using System.Diagnostics.CodeAnalysis;
using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController
{
    private View _view;
    private string _teamsFolder;
    private const string SEPARATOR = "----------------------------------------";

    private int _inputFromUser;
    private bool wasCanceled = false;
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
        game.CurrentTeam.RealoadTurns();
        game.OtherTeam.RealoadTurns();
        LoadSingleValueForView(game.PlayerTurnExclamation(game.CurrentTeam));

        while (game.CurrentTeam.State == TeamState.WithTurn && game.CurrentTeam.GetCurrentFullTurns() != 0 && game.handleGameFinished() == null)
        {
            LoadListValuesForView(game.TeamsUnitsCurrentStatus());
            LoadListValuesForView(game.CurrentTeam.CurrentTurnsbyType());
            LoadSingleValueForView(SEPARATOR);
            LoadListValuesForView(game.CurrentTeam.CurrentTurnOrder());
            LoadSingleValueForView(SEPARATOR);
            // Cada que se cancela debe volver por aca, hay que ver esooo, igual con un bucle, pero ya lo vamos viendo
            wasCanceled = true;

            while (wasCanceled)
            {
                LoadListValuesForView(game.CurrentTeam.WhoAttack().SelectOptions());
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

        var partida = new Game();
        var result = partida.TeamCreation(lines);
        _view.WriteLine(result);
        Console.WriteLine("HOLA");
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

    private void HandleAction(Game game)
    {
        wasCanceled = false;
        switch (InputFromUser)
        {
            
            case 1:
                LoadSingleValueForView($"Seleccione un objetivo para {game.CurrentTeam.WhoAttack().Name}");
                LoadListValuesForView(game.OtherTeam.ShowSelectablesUnit());
                InputText(_view.ReadLine());
                if (InputFromUser == game.OtherTeam.GetNumberUnitsInStartingTeam() + 1)
                    wasCanceled = true;
                else
                {
                    Unit attacked = game.OtherTeam.GetSelectableUnits()[InputFromUser - 1];
                    int damageDone = game.CurrentTeam.WhoAttack()
                        .AttackKinds(attacked, ElementType.Physics);
                    LoadListValuesForView(game.AttackLogs(damageDone, game.CurrentTeam.WhoAttack(),
                        attacked, ElementType.Physics)); 
                    game.CurrentTeam.ChangeOrder();

                    LoadListValuesForView(game.CurrentTeam.TurnUsed());
                    LoadSingleValueForView(SEPARATOR);

                }
                
                break;

            case 2:
                LoadSingleValueForView($"Seleccione un objetivo para {game.CurrentTeam.WhoAttack().Name}");
                LoadListValuesForView(game.OtherTeam.ShowSelectablesUnit());
                InputText(_view.ReadLine());
                
                if (InputFromUser == game.OtherTeam.GetNumberUnitsInStartingTeam() + 1)
                    wasCanceled = true;
                
                else
                {
                    
                    Unit attacked = game.OtherTeam.GetSelectableUnits()[InputFromUser - 1];
                    int damageDone = game.CurrentTeam.WhoAttack()
                        .AttackKinds(attacked, ElementType.Gun);
                    LoadListValuesForView(game.AttackLogs(damageDone, game.CurrentTeam.WhoAttack(),
                        attacked, ElementType.Gun)); 
                    game.CurrentTeam.ChangeOrder();
                    
                    LoadListValuesForView(game.CurrentTeam.TurnUsed());
                    LoadSingleValueForView(SEPARATOR);

                }
                
                
                

                break;
            case 3:
                LoadSingleValueForView($"Seleccione una habilidad para que {game.CurrentTeam.WhoAttack().Name} use");
                LoadListValuesForView(game.CurrentTeam.WhoAttack().ShowSelectableAbilities());
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
                LoadSingleValueForView($"{game.CurrentTeam.Name()} se rinde");
                
                LoadSingleValueForView(SEPARATOR);
                break;
        }
    }
}
