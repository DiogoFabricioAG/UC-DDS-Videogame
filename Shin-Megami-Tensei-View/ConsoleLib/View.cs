using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_Model.Extensions;
using Shin_Megami_Tensei_View.ConsoleLib;

namespace Shin_Megami_Tensei_View;

public class View
{
    private const string SEPARATOR = "----------------------------------------";
    private readonly char[] LABELMAXUNITSONTABLE = { 'A', 'B', 'C', 'D' };

    private readonly AbstractView _view;

    public static View BuildConsoleView()
        => new View(new ConsoleView());

    public static View BuildTestingView(string pathTestScript)
        => new View(new TestingView(pathTestScript));

    public static View BuildManualTestingView(string pathTestScript)
        => new View(new ManualTestingView(pathTestScript));
    
    private View(AbstractView newView)
    {
        _view = newView;
    }
    
    public string ReadLine()
    {
        return _view.ReadLine();
    }

    public void WriteLine(string message)
    {
        _view.WriteLine(message);
    }
    
    public void DisplayUnitActions(string unitName, List<ActionType> actions)
    {
        WriteLine($"Seleccione una acción para {unitName}");
        for (int i = 0; i < actions.Count; i++)
        {
            WriteLine($"{i + 1}: {actions[i].GetDescription()}");
        }
    }
    public void DisplayShowSelectableAbilities(Unit unit)
    {
        int counter = 1;
        foreach (var ability in unit.Ability.Where(x => x != null).ToArray())
        {
            WriteLine($"{counter}-{ability.Presentation()}");
            counter++;
        }
        WriteLine($"{counter}-Cancelar") ;
    }
    
    public void DisplayCurrentTurnOrder(Team team)
    {
        WriteLine("Orden:");
        for (int i = 0; i < team.GetNumberUnitsInStartingTeam(); i++)
            WriteLine($"{i + 1}-{team.OrderTeam.Where(x => x != null).ToArray()[(i + team.OrderAttack)%team.GetNumberUnitsInStartingTeam()].Name}") ;
        WriteLine(SEPARATOR);
    }
    
    
    // Para la Vista
    public void DisplayShowSelectablesUnit(Team team)
    {
        var counterUnit = 1;
        foreach (var unit in team.StartingTeam.Where(x => (x != null && x.Attributes.CurrentHp > 0)))
        {
            WriteLine($"{counterUnit}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp}");
            counterUnit++;
        }

        WriteLine($"{team.GetNumberUnitsInStartingTeam() + 1}-Cancelar");
    }
    
    // Para la Vista
    public void DisplayCurrentTurnsbyType(Team team)
    {
        WriteLine($"Full Turns: {team.GetCurrentFullTurns()}");
        WriteLine($"Blinking Turns: {team.GetCurrentBlinkingTurn()}");
        WriteLine(SEPARATOR);
    }
    
    public void DisplayPlayerTurnExclamation(Team team)
    {
        WriteLine($"Ronda de {team.Name()}\n{SEPARATOR}");
    } 

 
    public void DisplayTeamsUnitsCurrentStatus(Game game)
    {
        WriteLine($"Equipo de {game.Team1.Name()}");
       
        for (int i = 0; i < LABELMAXUNITSONTABLE.Length; i++)
        {
            if (game.Team1.StartingTeam[i] != null)
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-{game.Team1.StartingTeam[i].Status()}");
            }
            else
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-");
            }
        }; 
        WriteLine($"Equipo de {game.Team2.Name()}");
        for (int i = 0; i < LABELMAXUNITSONTABLE.Length; i++)
        {
            if (game.Team2.StartingTeam[i] != null)
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-{game.Team2.StartingTeam[i].Status()}");
            }
            else
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-");
            }
          
        }; 
        WriteLine(SEPARATOR);
    }
    
    
    // PARA LA VISTA
    public void DisplayAttackLogs(int attackDamage, Unit attacker, Unit attacked, ElementType type )
    {
        var typeAttackLog = type == ElementType.Gun ? "dispara" : "ataca";
        WriteLine($"{attacker.Name} {typeAttackLog} a {attacked.Name}");
        WriteLine($"{attacked.Name} recibe {attackDamage} de daño");
        WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
        WriteLine(SEPARATOR);
    }
    
    public void TurnUsedDisplay()
    {
        WriteLine("Se han consumido 1 Full Turn(s) y 0 Blinking Turn(s)");
        WriteLine("Se han obtenido 0 Blinking Turn(s)");
        WriteLine(SEPARATOR);
    }

    public void SurrenderTeamDisplay(Team team)
    {
        _view.WriteLine($"{team.Name()} se rinde");
        _view.WriteLine(SEPARATOR);
    }
    
    public string[] GetScript()
        => _view.GetScript();
}