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


    
    public Unit GetAttacker() => CurrentTeam.GetUnitInTurn();
    public Unit GetAttacked(int indexTarget, Team targetTeam, bool includeDefeated = false) => targetTeam.FindTargetUnit(indexTarget, includeDefeated);

    private bool ExistBlinkingTurn() => CurrentTeam.Turns.Exists(t => t != null && t.Type == TurnType.Blinking);
   

    public TurnType PassTurn()
    {
        CurrentTeam.ChangeOrder();
        CurrentTeam.CheckTurnRemains();
        if (ExistBlinkingTurn())
        {
            CurrentTeam.RemoveTurns(TurnType.Blinking);
            CurrentTeam.CheckTurnRemains();
            return TurnType.Blinking;
        }
        CurrentTeam.RemoveTurns(TurnType.Full);
        CurrentTeam.AddTurns(TurnType.Blinking);
        CurrentTeam.CheckTurnRemains();

        return TurnType.Full;
        
    }
    
    public bool IsGameFinished()
    {
        return GetWinningTeam() != null;
    }
    
    private static bool IsTeamSurrendered(Team team) => team.State == TeamState.Surrendered; 
    private static bool IsTeamDefeated(Team team) => team.State == TeamState.Defeated;
    public Team? GetWinningTeam()
    {
        if (IsTeamSurrendered(CurrentTeam))
        {
            return OtherTeam;
        }

        return IsTeamDefeated(OtherTeam) ? CurrentTeam : 
            IsTeamDefeated(CurrentTeam) ? OtherTeam : 
            null;
    }
}