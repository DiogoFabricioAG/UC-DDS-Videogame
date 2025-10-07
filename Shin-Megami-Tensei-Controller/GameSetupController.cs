using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameSetupController(View view, string teamsFolder, TeamController teamController)
{
     private const string SEPARATOR = "----------------------------------------";
    private const string ERROR_MESSAGE = "Archivo de equipos inválido";

    // LoadGame is now its own responsibility
    public string LoadGame(Game game)
    {
        view.WriteLine("Elige un archivo para cargar los equipos");
        if (Directory.Exists(teamsFolder))
        {
            try
            {
                var files = Directory.GetFiles(teamsFolder);
                var counter = 0;
                foreach (var archivo in files)
                {
                    view.WriteLine($"{counter}: {Path.GetFileName(archivo)}");
                    counter++;
                }
                
            }
            catch (Exception ex)
            {
                view.WriteLine($"Ocurrió un error al leer la carpeta: {ex.Message}");
            }
        }
        else
        {
            view.WriteLine("La carpeta de equipos no existe.");
        }
        var input = view.ReadLine();
        var selection = Convert.ToInt32(input);
        var selectedFile= Directory.GetFiles(teamsFolder)[selection];
        var lines = File.ReadAllLines(selectedFile);
        var result = TeamCreation(lines, game.CurrentTeam, game.OtherTeam);
        return result;
    }

    // TeamCreation and helpers are moved here (already clean)
    private string TeamCreation(string[] lines, Team team1, Team team2)
    {
        var lineups = ExtractTeamLineups(lines);
        var team1Lines = lineups.team1Lines;
        var team2Lines = lineups.team2Lines;

        var result1 = ConfigureTeam(team1Lines, team1, "1", TeamState.WithTurn);
        if (result1 != SEPARATOR)
        {
            return result1; 
        }

        var result2 = ConfigureTeam(team2Lines, team2, "2", TeamState.WithoutTurn);
        return result2 != SEPARATOR ? result2 : SEPARATOR;
    }
    private (string[] team1Lines, string[] team2Lines) ExtractTeamLineups(string[] lines)
    {
        var position2 = 1;
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains("Player 2")) continue;
            position2 = i;
            break;
        }
        
        var team1Lines = lines.Skip(1).Take(position2 - 1).ToArray();
        var team2Lines = lines.Skip(position2 + 1).Take(lines.Length - position2 - 1).ToArray();

        return (team1Lines, team2Lines);
    }
    
    private string ConfigureTeam(string[] alineations, Team team, string identifier, TeamState initialState)
    {
        var hasError = teamController.EnterUnits(alineations, team);
        
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
}