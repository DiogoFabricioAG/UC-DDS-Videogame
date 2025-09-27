namespace Shin_Megami_Tensei_Model;

public class Game
{
    private Team _currentTeam;
    
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
        if (HandleGameFinished() == null)
        {
            (CurrentTeam, OtherTeam) = (OtherTeam, CurrentTeam);
            CurrentTeam.State = TeamState.WithTurn;
            OtherTeam.State = TeamState.WithoutTurn;
        }
    }
    
    public Game()
    {
        CurrentTeam = new Team
        {
            NumberTeam = 1
        };
        OtherTeam = new Team
        {
            NumberTeam = 2
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
        var team1 = CurrentTeam.NumberTeam == 1 ? CurrentTeam : OtherTeam;
        var team2 = CurrentTeam.NumberTeam == 2 ? CurrentTeam : OtherTeam;
        return (team1, team2);
    }

    public (Unit, Unit) GetAttackerAndTarget(int indexTarget)
    {
        var attacker = CurrentTeam.WhoAttack();
        var attacked = OtherTeam.GetSelectableUnits()[indexTarget-1];
        return (attacker, attacked);
    }

    public TurnType PassTurn()
    {
        CurrentTeam.ChangeOrder();
        if (CurrentTeam.Turns.Exists(t => t != null && t.Type == TurnType.Blinking))
        {
            CurrentTeam.DestroyTurn(TurnType.Blinking);
            CurrentTeam.TurnRemains();
            return TurnType.Blinking;
        }
        CurrentTeam.DestroyTurn(TurnType.Full);
        CurrentTeam.AddTurn(TurnType.Blinking);
        CurrentTeam.TurnRemains();

        return TurnType.Full;
        
    }
    
    public Team? HandleGameFinished()
    {
        if (CurrentTeam.State == TeamState.Surrendered)
        {
            return OtherTeam;
        }

        return OtherTeam.State == TeamState.Defeated ? CurrentTeam : null;
    }
}