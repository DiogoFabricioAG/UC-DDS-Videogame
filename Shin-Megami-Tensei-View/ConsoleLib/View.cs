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
        foreach (var ability in unit.Abilities.Where(x => x != null && unit.Attributes.CurrentMp > x.Cost).ToArray())
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
            WriteLine($"{i + 1}-{team.OrderForActions.Where(x => x != null && x.Attributes.CurrentHp > 0).ToArray()[(i + team.OrderAttack)%team.GetNumberUnitsInStartingTeam()].Name}") ;
        WriteLine(SEPARATOR);
    }
    
    
    public void DisplayShowSelectablesUnit(Team otherTeam,Team currentTeam, TargetType targetType, bool showThemAll = false)
    {
        WriteLine($"Seleccione un objetivo para {currentTeam.GetUnitInTurn().Name}");
        var counterUnit = 1;
        var unitsSelected = showThemAll ? currentTeam.GetDefeatedUnits() : targetType == TargetType.Ally ? 
            currentTeam.GetSelectableUnits(showThemAll)
            : otherTeam.GetSelectableUnits(showThemAll);

        foreach (var unit in unitsSelected)
        {
            WriteLine($"{counterUnit}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp}");
            counterUnit++;
        }

        var finalOption = showThemAll ? $"{currentTeam.GetDefeatedUnits().Length + 1}-Cancelar" : targetType == TargetType.Ally
            ? $"{currentTeam.GetNumberUnitsInStartingTeam() + 1}-Cancelar"
            : $"{otherTeam.GetNumberUnitsInStartingTeam() + 1}-Cancelar";
        WriteLine(finalOption);
    }
    
    public void DisplayCurrentTurnsbyType(Team team)
    {
        WriteLine($"Full Turns: {team.GetCurrentFullTurns()}");
        WriteLine($"Blinking Turns: {team.GetCurrentBlinkingTurns()}");
        WriteLine(SEPARATOR);
    }
    
    public void DisplayPlayerTurnExclamation(Team team)
    {
        WriteLine($"Ronda de {team.Name()}\n{SEPARATOR}");
    }

    public void DisplayAbilitiesForUnit(Unit unit)
    {
        var allAbilities = unit.Abilities.Where(x => x != null).ToArray();
        for (var i = 0; i < allAbilities.Length; i++)
        {
            WriteLine($"{i+1}-{allAbilities[i].Name} MP:{allAbilities[i].Cost}");
        }
    }
 
    public void DisplayTeamsUnitsCurrentStatus(Game game)
    {
        var (team1, team2) = game.GetPlayer1AndPlayer2();
        WriteLine($"Equipo de {team1.Name()}");
       
        for (int i = 0; i < LABELMAXUNITSONTABLE.Length; i++)
        {
            if (team1.StartingTeam[i] != null)
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-{team1.StartingTeam[i].Status()}");
            }
            else
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-");
            }
        }; 
        WriteLine($"Equipo de {team2.Name()}");
        for (int i = 0; i < LABELMAXUNITSONTABLE.Length; i++)
        {
            if (team2.StartingTeam[i] != null)
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-{team2.StartingTeam[i].Status()}");
            }
            else
            {
                WriteLine($"{LABELMAXUNITSONTABLE[i]}-");
            }
          
        }; 
        WriteLine(SEPARATOR);
    }
    
    public void DisplayAbilityLogs(int attackDamage, Unit attacker, Unit attacked, AffinityType type, AbilityType abilityType, int numberHits, bool reviveUnit = false )
    {
        var affinityText = type == AffinityType.Weak ? "débil contra" : type == AffinityType.Resist ? "resistente" : string.Empty;
        var attackType = "";
        switch (abilityType)
        {
            case  AbilityType.Phys:
                attackType = "ataca";
                break;
            case AbilityType.Gun:
                attackType = "dispara";
                break;
            case AbilityType.Fire:
                attackType = "lanza fuego";
                break;
            case AbilityType.Ice:
                attackType = "lanza hielo";
                break;
            case AbilityType.Elec:
                attackType = "lanza electricidad";
                break;
            case AbilityType.Force:
                attackType = "lanza viento";
                break;
            case AbilityType.Light:
                attackType = "ataca con luz";
                break;
            case AbilityType.Dark:
                attackType = "ataca con oscuridad";
                break;
            case AbilityType.Heal:
                
                attackType = reviveUnit ? "revive" : "cura";
                break;
        }

        var healOrDamage = abilityType != AbilityType.Heal ? "daño" : "HP";
        
        for (var i = 0; i < numberHits; i++)
        {
            WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");
            switch (type)
            {
                case AffinityType.Weak:
                case AffinityType.Resist:
                    WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");
                    WriteLine($"{attacked.Name} recibe {attackDamage} de daño");
                    break;
                case AffinityType.Null:
                    WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                    break;
                case AffinityType.Repel:
                    WriteLine($"{attacked.Name} devuelve {attackDamage} daño a {attacker.Name}");
                    break;
                case AffinityType.Drain:
                    WriteLine($"{attacked.Name} absorbe {Math.Abs(attackDamage)} daño");
                    break;
                default:
                    WriteLine($"{attacked.Name} recibe {attackDamage} de {healOrDamage}");
                    break;
            }
        }
        
        if (type == AffinityType.Repel)
        {
            WriteLine($"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}");
        }
        else
        {
            WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
        }
        WriteLine(SEPARATOR);
    }
    
    public void TurnUsedDisplayWithParameters(int blinkingTurnLoss, int fullTurnLoss, int blinkingTurnWon)
    {
        WriteLine($"Se han consumido {fullTurnLoss} Full Turn(s) y {blinkingTurnLoss} Blinking Turn(s)");
        WriteLine($"Se han obtenido {blinkingTurnWon} Blinking Turn(s)");
        WriteLine(SEPARATOR);
    }
    
    public void SurrenderTeamDisplay(Team team)
    {
        _view.WriteLine($"{team.Name()} se rinde");
        _view.WriteLine(SEPARATOR);
    }
    
    public void ShowInvocableMonsters(Team team, bool showAll = false)
    {
        WriteLine("Seleccione un monstruo para invocar");
        
        int counter = 1;

        foreach (var unit in team.GetMonstersInBackup(showAll))
        {
            WriteLine($"{counter}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp}");
            counter++;
        }
        WriteLine($"{counter}-Cancelar");
    }

    public void ShowReplaceableUnits(Team team)
    {
        WriteLine("Seleccione una posición para invocar");
        int counter = 1;
        foreach (var unit in team.GetReplaceableTeam())
        {
            if (unit != null && unit.Attributes.CurrentHp > 0)
            {
                WriteLine(
                    $"{counter}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp} (Puesto {team.KnowIndexFromUnitInStartingTeam(unit) + 1})");
            }
            else
            {
                WriteLine(
                    $"{counter}-Vacío (Puesto {counter + 1})");
            }
            counter++;
        }
        WriteLine($"{counter}-Cancelar");
    }

    public void InvokeAnUnit(Unit unit, bool revived = false, Unit inTurn = null)
    {
        
        WriteLine($"{unit.Name} ha sido invocado");
        if (revived)
        {
            WriteLine($"{inTurn.Name} revive a {unit.Name}");
            WriteLine($"{unit.Name} recibe {unit.Attributes.MaxHp} de HP");
            WriteLine($"{unit.Name} termina con HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp}");
        }
        WriteLine(SEPARATOR);
    }
    
    public string[] GetScript()
        => _view.GetScript();
}