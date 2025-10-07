namespace Shin_Megami_Tensei_Model;

public class Game
{
    private const int nTeam1 = 1;
    private const int nTeam2 = 2;
    public Team CurrentTeam { get; private set; } 
    public Team OtherTeam { get; private set; }

    public void ChangeCurrentTeam()
    {
        if (IsGameFinished()) return;
        (CurrentTeam, OtherTeam) = (OtherTeam, CurrentTeam);
        CurrentTeam.State = TeamState.WithTurn;
        OtherTeam.State = TeamState.WithoutTurn;
    }
    
    public Game()
    {
        CurrentTeam = new Team
        {
            NumberTeam = nTeam1
        };
        OtherTeam = new Team
        {
            NumberTeam = nTeam2
        };
        CurrentTeam.State = TeamState.WithTurn;
        OtherTeam.State = TeamState.WithoutTurn;
    }

    public void HandleSurrender()
    {
        CurrentTeam.State = TeamState.Surrendered;
    }

    public (Team, Team) GetPlayer1AndPlayer2()
    {
        var team1 = CurrentTeam.NumberTeam == nTeam1 ? CurrentTeam : OtherTeam;
        var team2 = CurrentTeam.NumberTeam == nTeam2 ? CurrentTeam : OtherTeam;
        return (team1, team2);
    }

    public (Unit, Unit) GetAttackerAndTarget(int indexTarget, Team targetTeam, bool includeDefeated = false)
    {
        var attacker = CurrentTeam.GetUnitInTurn();
        var attacked = targetTeam.FindTargetUnit(indexTarget, includeDefeated);
        return (attacker, attacked);
    }

   

    public TurnType PassTurn()
    {
        CurrentTeam.ChangeOrder();
        if (CurrentTeam.Turns.Exists(t => t != null && t.Type == TurnType.Blinking))
        {
            CurrentTeam.RemoveTurns(TurnType.Blinking);
            CurrentTeam.TurnRemains();
            return TurnType.Blinking;
        }
        CurrentTeam.RemoveTurns(TurnType.Full);
        CurrentTeam.AddTurns(TurnType.Blinking);
        CurrentTeam.TurnRemains();

        return TurnType.Full;
        
    }
    
    public bool IsGameFinished()
    {
        return GetWinningTeam() != null;
    }
    
    public Team? GetWinningTeam()
    {
        if (CurrentTeam.State == TeamState.Surrendered)
        {
            return OtherTeam;
        }

        return OtherTeam.State == TeamState.Defeated ? CurrentTeam : 
            CurrentTeam.State == TeamState.Defeated ? OtherTeam : 
            null;
    }
}