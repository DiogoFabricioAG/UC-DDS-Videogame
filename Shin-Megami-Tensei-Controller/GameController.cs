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
        game.currentTeam.RealoadTurns();
        game.otherTeam.RealoadTurns();
        LoadSingleValueForView(game.PlayerTurnExclamation(game.currentTeam));

        while (game.currentTeam.State == TeamState.WithTurn && game.currentTeam.GetCurrentFullTurns() != 0 && game.handleGameFinished() == null)
        {
            LoadListValuesForView(game.TeamsUnitsCurrentStatus());
            LoadListValuesForView(game.currentTeam.CurrentTurnsbyType());
            LoadSingleValueForView(SEPARATOR);
            LoadListValuesForView(game.currentTeam.CurrentTurnOrder());
            LoadSingleValueForView(SEPARATOR);
            // Cada que se cancela debe volver por aca, hay que ver esooo, igual con un bucle, pero ya lo vamos viendo
            wasCanceled = true;

            while (wasCanceled)
            {
                LoadListValuesForView(game.currentTeam.WhoAttack().SelectOptions());
                InputText(_view.ReadLine());
                HandleAction(game);
                if (InputFromUser == 6)
                {
                    break;
                }

            }
            
            game.otherTeam.AnyUnitDestroyed();
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

    private void HandleAction(Game game)
    {
        wasCanceled = false;
        switch (InputFromUser)
        {
            
            case 1:
                LoadSingleValueForView($"Seleccione un objetivo para {game.currentTeam.WhoAttack().Name}");
                LoadListValuesForView(game.otherTeam.ShowSelectablesUnit());
                InputText(_view.ReadLine());
                if (InputFromUser == game.otherTeam.GetNumberUnitsInStartingTeam() + 1)
                    wasCanceled = true;
                else
                {
                    Unit attacked = game.otherTeam.GetSelectableUnits()[InputFromUser - 1];
                    int damageDone = game.currentTeam.WhoAttack()
                        .AttackKinds(attacked, ElementType.Physics);
                    LoadListValuesForView(game.AttackLogs(damageDone, game.currentTeam.WhoAttack(),
                        attacked, ElementType.Physics)); 
                    game.currentTeam.ChangeOrder();

                    LoadListValuesForView(game.currentTeam.TurnUsed());
                    LoadSingleValueForView(SEPARATOR);

                }
                
                break;

            case 2:
                LoadSingleValueForView($"Seleccione un objetivo para {game.currentTeam.WhoAttack().Name}");
                LoadListValuesForView(game.otherTeam.ShowSelectablesUnit());
                InputText(_view.ReadLine());
                
                if (InputFromUser == game.otherTeam.GetNumberUnitsInStartingTeam() + 1)
                    wasCanceled = true;
                
                else
                {
                    
                    Unit attacked = game.otherTeam.GetSelectableUnits()[InputFromUser - 1];
                    int damageDone = game.currentTeam.WhoAttack()
                        .AttackKinds(attacked, ElementType.Gun);
                    LoadListValuesForView(game.AttackLogs(damageDone, game.currentTeam.WhoAttack(),
                        attacked, ElementType.Gun)); 
                    game.currentTeam.ChangeOrder();
                    
                    LoadListValuesForView(game.currentTeam.TurnUsed());
                    LoadSingleValueForView(SEPARATOR);

                }
                
                
                

                break;
            case 3:
                LoadSingleValueForView($"Seleccione una habilidad para que {game.currentTeam.WhoAttack().Name} use");
                LoadListValuesForView(game.currentTeam.WhoAttack().ShowSelectableAbilities());
                InputText(_view.ReadLine());
                Console.WriteLine("Numero de Habilidades: " + game.currentTeam.WhoAttack().GetTotalAbilities());
                if (InputFromUser == game.currentTeam.WhoAttack().GetTotalAbilities() + 1)
                {
                    wasCanceled = true;
                    game.currentTeam.ChangeOrder();
                }
                break;
            case 6:
                game.HandleSurrender();
                LoadSingleValueForView($"{game.currentTeam.Name()} se rinde");
                
                LoadSingleValueForView(SEPARATOR);
                break;
        }
    }
}
