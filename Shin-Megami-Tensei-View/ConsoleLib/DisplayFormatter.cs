using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_View.ConsoleLib;

public class DisplayFormatter
{
    private readonly char[] LABEL_MAX_UNITS_ON_TABLE = { 'A', 'B', 'C', 'D' };
    public List<string> FormatTurnOrder(Team team) {
        var listLogs = new List<string>();
        for (var i = 0; i < team.GetNumberUnitsInStartingTeam(); i++)
            listLogs.Add($"{i + 1}-{team.OrderForActions.Where(x => x != null && x.Attributes.CurrentHp > 0).ToArray()[(i + team.TeamTurnOrder)%team.GetNumberUnitsInStartingTeam()].Name}") ;
        return listLogs;
    }

    public (List<string> unitsText, string cancelOption) FormatSelectableUnits(Team otherTeam, Team currentTeam, TargetType targetType, bool showThemAll)
    {
        var listLogs = new List<string>();
        var unitsSelected = showThemAll ? currentTeam.GetDefeatedUnits() : targetType == TargetType.Ally ? 
            currentTeam.GetSelectableUnits(showThemAll)
            : otherTeam.GetSelectableUnits(showThemAll);
        var counterUnit = 1;
        foreach (var unit in unitsSelected)
        {
            listLogs.Add($"{counterUnit}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp}");
            counterUnit++;
        }
        var finalOption = showThemAll ? $"{currentTeam.GetDefeatedUnits().Length + 1}-Cancelar" : targetType == TargetType.Ally
            ? $"{currentTeam.GetNumberUnitsInStartingTeam() + 1}-Cancelar"
            : $"{otherTeam.GetNumberUnitsInStartingTeam() + 1}-Cancelar";
        
        return (listLogs, finalOption);
    }

    public List<string> FormatTeamsStatusTable(Team team1, Team team2)
    {
        var  listLogs = new List<string> { $"Equipo de {team1.GetName()}" };
        listLogs.AddRange(LABEL_MAX_UNITS_ON_TABLE.Select((t, i) => team1.StartingTeam[i] != null
                ? $"{t}-{team1.StartingTeam[i].GetStatus()}"
                : $"{t}-"));
        
        listLogs.Add($"Equipo de {team2.GetName()}");
        listLogs.AddRange(LABEL_MAX_UNITS_ON_TABLE.Select((t, i) => team2.StartingTeam[i] != null
            ? $"{t}-{team2.StartingTeam[i].GetStatus()}"
            : $"{t}-"));
        return listLogs; 
    }

    public List<string> FormatReplaceableUnits(Team target)
    {
        List<string> listLogs = [];
        var counter = 1;
        foreach (var unit in target.GetReplaceableTeam())
        {
            if (unit != null && unit.Attributes.CurrentHp > 0)
            {
                listLogs.Add(
                    $"{counter}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp} (Puesto {target.KnowIndexFromUnitInStartingTeam(unit) + 1})");
            }
            else
            {
                listLogs.Add(
                    $"{counter}-Vacío (Puesto {counter + 1})");
            }
            counter++;
        }

        listLogs.Add($"{counter}-Cancelar");
        return listLogs;
    }

    public List<string> FormatInvocableUnits(Team target, bool showDefeatUnitsToo)
    {
        var counter = 1;
        List<string> listLogs = [];
        foreach (var unit in target.GetMonstersInBackup(showDefeatUnitsToo))
        {
            listLogs.Add($"{counter}-{unit.Name} HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp} MP:{unit.Attributes.CurrentMp}/{unit.Attributes.MaxMp}");
            counter++;
        }
        listLogs.Add($"{counter}-Cancelar");
        return listLogs;
    }

   
}