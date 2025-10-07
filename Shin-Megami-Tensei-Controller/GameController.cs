using System.Diagnostics.CodeAnalysis;
using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameController(View view, string teamsFolder, TeamController teamController)
{
    private const string ERROR_MESSAGE = "Archivo de equipos inválido"; 

    private readonly GameSetupController _setupService = new(view, teamsFolder, teamController); 
    private readonly CombatController _combatOrchestrator = new(view);
    
    public void Play()
    {
        var game = new Game();
        var resultLoad = _setupService.LoadGame(game);
        
        view.WriteLine(resultLoad);
        if (resultLoad == ERROR_MESSAGE)
        {
            return; 
        }

        RunGameLoop(game);
        view.WriteLine($"Ganador: {game.GetWinningTeam()?.GetName()}");
    }
    private void RunGameLoop(Game game)
    {
        while (!game.IsGameFinished())
        {
            _combatOrchestrator.HandleChangeTurn(game); 
        }
    }
}