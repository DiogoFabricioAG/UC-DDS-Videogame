namespace Shin_Megami_Tensei_Model;




public class Game

{
    private const string SEPARATOR = "----------------------------------------";
    private readonly char[] LABELMAXUNITSONTABLE = { 'A', 'B', 'C', 'D' };
    private Team team1 { get; set; }
    private Team team2 { get; set; }

    private Team _currentTeam;

    private bool _gameError;

    public bool GameError
    {
        get => _gameError;
        set => _gameError = value;
    }
    public Team CurrentTeam
    {
        get => _currentTeam;
        private set => _currentTeam = value;
    }
    private Team _otherTeam;

    public Team OtherTeam
    {
        get => _otherTeam;
        private set => _otherTeam = value;
    }
    
    public void ChangeCurrentTeam()
    {
        if (CurrentTeam == null || CurrentTeam != team1) {CurrentTeam = team1; OtherTeam = team2; }
        else if  (CurrentTeam == team1)
        {
            CurrentTeam = team2;
            OtherTeam = team1;
        };
        CurrentTeam.State = TeamState.WithTurn;
        OtherTeam.State = TeamState.WithoutTurn;
    }
    
    public Game()
    {
        team1 = new Team();
        team2 = new Team();
    }
    
    public string TeamCreation(string[] lines)
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
        var result = team1.EnterUnits(alineacionE1);
        team1.Identifier = "1";
        team1.State = TeamState.WithTurn;
        Console.WriteLine("EQUIPO 1:"+ result);
        if (result == "Archivo de equipos inválido")
        {
            return result;
        }
        team1.SelectStarterTeam();
        var result2 = team2.EnterUnits(alineacionE2);
        team2.Identifier = "2";
        team2.State = TeamState.WithoutTurn;
        Console.WriteLine("EQUIPO 2:"+ result2);

        if (result2 == "Archivo de equipos inválido")
        {
            return result2;
        }
        team2.SelectStarterTeam();
        return SEPARATOR;
    }

    
 
    public String PlayerTurnExclamation(Team team) => $"Ronda de {team.Name()}\n{SEPARATOR}";

    public String[] TeamsUnitsCurrentStatus()
    {
        // Adding a ';' for split later (sry for this attempt of english)
       string tempLog = $"Equipo de {team1.Name()}" + ";";
       
       for (int i = 0; i < LABELMAXUNITSONTABLE.Length; i++)
       {
           if (team1.StartingTeam[i] != null)
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-{team1.StartingTeam[i].Status()}" + ";";
           }
           else
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-" + ";";
           }
       }; 
       tempLog += $"Equipo de {team2.Name()}" + ";";
       for (int i = 0; i < LABELMAXUNITSONTABLE.Length; i++)
       {
           if (team2.StartingTeam[i] != null)
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-{team2.StartingTeam[i].Status()}" + ";";
           }
           else
           {
               tempLog += $"{LABELMAXUNITSONTABLE[i]}-" + ";";
           }
          
       }; 
       tempLog += SEPARATOR;
       return tempLog.Split(';');
    }


    public void HandleSurrender()
    {
        CurrentTeam.State = TeamState.Surrendered;
    }

    public Team? handleGameFinished()
    {
        if (CurrentTeam.State == TeamState.Surrendered)
        {
            return OtherTeam;
        }
        var wasDefeated = team1.GetNumberUnitsInStartingTeam() == 0 ;
        
        if (wasDefeated)
        {
            team2.State = TeamState.Defeated;
            return team2;
        };
        wasDefeated = team2.GetNumberUnitsInStartingTeam() == 0;

        if (wasDefeated)
        {
            team1.State = TeamState.Defeated;
            return team1;
        };
        return null;
    }
    
    

    public String[] AttackLogs(int attackDamage, Unit attacker, Unit attacked, ElementType type )
    {

        string typeAttackLog = type == ElementType.Gun ? "dispara" : "ataca";
        string tempLog = "";
        tempLog += $"{attacker.Name} {typeAttackLog} a {attacked.Name}" + ";";
        tempLog += $"{attacked.Name} recibe {attackDamage} de daño" + ";";
        tempLog += $"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}" + ";";
        tempLog += SEPARATOR;
        return tempLog.Split(';');
    }
}