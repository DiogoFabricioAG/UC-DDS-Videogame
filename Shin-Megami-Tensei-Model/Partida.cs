using System.Collections;

namespace Shin_Megami_Tensei_Model;




public class Partida
{
    private const string SEPARATOR = "----------------------------------------";
    private readonly char[] LABELMAXUNITSONTABLE = { 'A', 'B', 'C', 'D' };
    private Equipo equipo1 { get; set; }
    private Equipo equipo2 { get; set; }

    public Equipo currentTeam;

    public void ChangeCurrentTeam()
    {
        if (currentTeam == null || currentTeam != equipo1) currentTeam = equipo1;
        else if  (currentTeam == equipo1) currentTeam = equipo2;
    }
    
    public ArrayList Log = new ArrayList();
    public Partida()
    {
        equipo1 = new Equipo();
        equipo2 = new Equipo();
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
        var result = equipo1.IngresarEquipo(alineacionE1);
        equipo1.numero = "1";
        var result2 = equipo2.IngresarEquipo(alineacionE2);
        equipo2.numero = "2";


        if (result == "Archivo de equipos inválido")
        {
            return result;
        }
        if (result2 == "Archivo de equipos inválido")
        {
            return result2;
        }
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
    public String PlayerTurnExclamation(Equipo equipo) => $"Ronda de {equipo.Name()}\n{SEPARATOR}";

    public String[] TeamsUnitsCurrentStatus()
    {
        // Adding a ';' for split later (sry for this attempt of english)
       string tempLog = $"Equipo de {equipo1.Name()}" + ";";
       tempLog += $"{LABELMAXUNITSONTABLE[0]}-{equipo1.samurai.Status()}" + ";";
       
       for (int i = 1; i < LABELMAXUNITSONTABLE.Length; i++)
       {
           if (equipo1.getMonsterID() + 1 > i)
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-{equipo1.monstruos[i-1].Status()}" + ";";
           }
           else
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-" + ";";
           }
       }; 
       tempLog += $"Equipo de {equipo2.Name()}" + ";";
       tempLog += $"{LABELMAXUNITSONTABLE[0]}-{equipo2.samurai.Status()}" + ";";
       for (int i = 1; i < LABELMAXUNITSONTABLE.Length; i++)
       {
           if (equipo2.getMonsterID() +1 > i)
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-{equipo2.monstruos[i-1].Status()}" + ";";
           }
           else
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-" + ";";
           }
          
       }; 
       tempLog += SEPARATOR;
       return tempLog.Split(';');
    }

    
    
    
    public void GameStart()
    {
        ChangeCurrentTeam();
        LoadSingleValue(PlayerTurnExclamation(currentTeam));
        LoadListValues(TeamsUnitsCurrentStatus());
        LoadListValues(currentTeam.CurrentTurnsbyType());
        LoadSingleValue(SEPARATOR);
        LoadListValues(currentTeam.CurrentTurnOrder());
        LoadSingleValue(SEPARATOR);
        LoadListValues(currentTeam.samurai.selectOptions());
    }
    
}