namespace Shin_Megami_Tensei_Model;

public class Game
{
    private const string SEPARATOR = "----------------------------------------";
    public Team Team1 { get; set; }
    public Team Team2 { get; set; }

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
        if (CurrentTeam == null || CurrentTeam != Team1) {CurrentTeam = Team1; OtherTeam = Team2; }
        else if  (CurrentTeam == Team1)
        {
            CurrentTeam = Team2;
            OtherTeam = Team1;
        };
        CurrentTeam.State = TeamState.WithTurn;
        OtherTeam.State = TeamState.WithoutTurn;
    }
    
    public Game()
    {
        Team1 = new Team();
        Team2 = new Team();
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
        var wasDefeated = Team1.GetNumberUnitsInStartingTeam() == 0 ;
        
        if (wasDefeated)
        {
            Team2.State = TeamState.Defeated;
            return Team2;
        };
        wasDefeated = Team2.GetNumberUnitsInStartingTeam() == 0;

        if (wasDefeated)
        {
            Team1.State = TeamState.Defeated;
            return Team1;
        };
        return null;
    }
}