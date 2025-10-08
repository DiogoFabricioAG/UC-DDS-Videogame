using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
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
        var config1Ctx = new ConfigurationContext(team1Lines, team1, "1", TeamState.WithTurn);
        var result1 = ConfigureTeam(config1Ctx);
        if (result1 != SEPARATOR)
        {
            return result1; 
        }

        var config2Ctx = new ConfigurationContext(team2Lines, team2, "2", TeamState.WithoutTurn);
        var result2 = ConfigureTeam(config2Ctx);

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
    
    private string ConfigureTeam(ConfigurationContext ctx)
    {
        var hasError = teamController.EnterUnits(ctx.alineations, ctx.team);
        Console.WriteLine("ERRORES?: "  + hasError);
        if (hasError)
        {
            return ERROR_MESSAGE;
        }
        ctx.team.Identifier = ctx.identifier;
        ctx.team.State = ctx.initialState;
        
        TeamController.GenerateTeamForInitGame(ctx.team); 

        ctx.team.Samurai.ShowAbility(); 

        return SEPARATOR; 
    }
}