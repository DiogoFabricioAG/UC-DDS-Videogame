using System.Collections;

namespace Shin_Megami_Tensei_Model;




public class Game
{
    private const string SEPARATOR = "----------------------------------------";
    private readonly char[] LABELMAXUNITSONTABLE = { 'A', 'B', 'C', 'D' };
    private Team equipo1 { get; set; }
    private Team equipo2 { get; set; }

    public Team currentTeam;

    public void ChangeCurrentTeam()
    {
        if (currentTeam == null || currentTeam != equipo1) currentTeam = equipo1;
        else if  (currentTeam == equipo1) currentTeam = equipo2;
    }
    
    public ArrayList Log = new ArrayList();
    public Game()
    {
        equipo1 = new Team();
        equipo2 = new Team();
    }
    
    public string CreacionEquipo(string[] lines)
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
        var result = equipo1.EnterUnits(alineacionE1);
        equipo1.Identifier = "1";
        if (result == "Archivo de equipos inválido")
        {
            return result;
        }
        equipo1.SelectStarterTeam();
        Console.WriteLine("Resultado Equipo 1");
        var result2 = equipo2.EnterUnits(alineacionE2);
        equipo2.Identifier = "2";
        if (result2 == "Archivo de equipos inválido")
        {
            return result2;
        }
        equipo2.SelectStarterTeam();
        Console.WriteLine("Resultado Equipo 2");



        
        
        return SEPARATOR;
    }

    public void LoadListValues(string[] lines)
    {
        foreach (var line in lines)
        {
            Log.Add(line);
        }
    }

    public void LoadSingleValue(string line)
    {
        Log.Add(line);
    }
    public String PlayerTurnExclamation(Team team) => $"Ronda de {team.Name()}\n{SEPARATOR}";

    public String[] TeamsUnitsCurrentStatus()
    {
        // Adding a ';' for split later (sry for this attempt of english)
       string tempLog = $"Equipo de {equipo1.Name()}" + ";";
       tempLog += $"{LABELMAXUNITSONTABLE[0]}-{equipo1.Samurai.Status()}" + ";";
       
       for (int i = 1; i < LABELMAXUNITSONTABLE.Length; i++)
       {
           if (equipo1.MonsterId + 1 > i)
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-{equipo1.Monsters[i-1].Status()}" + ";";
           }
           else
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-" + ";";
           }
       }; 
       tempLog += $"Equipo de {equipo2.Name()}" + ";";
       tempLog += $"{LABELMAXUNITSONTABLE[0]}-{equipo2.Samurai.Status()}" + ";";
       for (int i = 1; i < LABELMAXUNITSONTABLE.Length; i++)
       {
           if (equipo2.MonsterId +1 > i)
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-{equipo2.Monsters[i-1].Status()}" + ";";
           }
           else
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-" + ";";
           }
          
       }; 
       tempLog += SEPARATOR;
       return tempLog.Split(';');
    }

    public Team? AnyTeamDefeated()
    {
        bool wasDefeated = true;
        foreach (var unit in equipo1.StartingTeam)
        {
            if (unit.Attributes.CurrentHp > 0)
            {
                wasDefeated = false;
                break;
            }
        }
        if (wasDefeated) return equipo1;

        foreach (var unit in equipo2.StartingTeam)
        {
            if (unit.Attributes.CurrentHp > 0)
            {

                wasDefeated = false;
                break;
            }
        }
        if (wasDefeated) return equipo2;
        else return null;
    }



    public String[] AttackLogs(int attackDamage, Unit attacker, Unit attacked)
    {
        string tempLog = SEPARATOR + ";";
        tempLog += $"{attacker.Name} ataca a {attacked.Name}" + ";";
        tempLog += $"{attacked.Name} recibe {attackDamage} de daño" + ";";
        tempLog += $"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}";
        return tempLog.Split(';');
    }
    
    
    
    
}