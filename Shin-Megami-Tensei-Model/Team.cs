
namespace Shin_Megami_Tensei_Model;

public class Team
{

    private const int CANTIDADMAXIMAMONSTRUOS = 7;
    private const int TOTALMONSTERINTABLE = 3;
    private const int MAXUNITSINTABLE = 4;

    public int OrderAttack { get; set; } = 0;
    public string Identifier { get; set; } = "0";
    public Samurai Samurai { get; set; } = new Samurai();
    public Monster[] Monsters { get; set; } = new Monster[CANTIDADMAXIMAMONSTRUOS];
    public List<Turn> Turns { get; set; } = new List<Turn>();

    private Unit[] _startingTeams = new Unit[MAXUNITSINTABLE];
    
    private TeamState _state = TeamState.Initialized;
    public TeamState State { get  => _state; set => _state = value; }
    
    public Unit[] StartingTeam
    {
        get => _startingTeams;
        set => _startingTeams = value;
    }
    
    private Unit[] _orderForActions = new Unit[MAXUNITSINTABLE];

    public Unit[] OrderForActions
    {
        get => _orderForActions;
        set => _orderForActions = value;
    }
    
    private bool _error;
    private int _monsterId = 0;
    public int  MonsterId { get => _monsterId; set => _monsterId = value; }

    
    public int NumberTeam { get; set; }
    
    public void ChangeOrder()
    {
        OrderAttack++;
        if (OrderAttack == GetNumberUnitsInStartingTeam())
        {
            OrderAttack = 0;
            State = TeamState.WithoutTurn;
        }
        State = TeamState.WithTurn;
    }

    public void TurnRemains()
    {
        if (!Turns.Any(t => t != null))
        {
            State = TeamState.WithoutTurn;
        }
    }

    public int GetCancelOptionAbilities() => WhoAttack().GetTotalAbilities() + 1;
    public int CancelOptionInSelectableTeam() => GetNumberUnitsInStartingTeam() + 1;
    public void WasDefeated()
    {
        if (GetNumberUnitsInStartingTeam() == 0)
        {
            State = TeamState.Defeated;
        }
        
    }
    
    public static string? FromInputGetSamuraiName(string line) => line.Split(' ')[1];


    public void AnyUnitDestroyed()
    {
        Unit unitDestroy = null;
        foreach (var unit in StartingTeam.Where(unit => unit != null).ToArray())
        {
            if (unit.Attributes.CurrentHp == 0)
            {
                if (_orderForActions.Contains(unit))
                {
                    unitDestroy = unit;
                    break;
                }
            }
        }

        if (unitDestroy != null)
        {
            if (StartingTeam[Array.IndexOf(StartingTeam, unitDestroy)] is Monster)
            {
                StartingTeam[Array.IndexOf(StartingTeam, unitDestroy)] = null;
            }
            if (Array.IndexOf(OrderForActions, unitDestroy) != -1)
                OrderForActions[Array.IndexOf(OrderForActions, unitDestroy)] = null;

        }
    }

    public void AddTurn(TurnType turnType)
    {
        Turns.Add(new Turn(turnType));
    }
    
    public void SelectStarterTeam()
    {
        if (Samurai == null)
        {
            _error = true;
            return;
        }
        StartingTeam[0] = Samurai;
        
        for (int i = 0; i < Math.Min(GetNumberAliveMonsters(), TOTALMONSTERINTABLE); i++)
        {
            StartingTeam[i + 1] = Monsters[i];
        }
        
        OrderForActions = StartingTeam.Where(x => x != null).OrderByDescending(x => x.Attributes.Speed).ToArray();
    }
    public string[] FromInputGetAbilities(string lineText)
    {
        int startIndex = lineText.IndexOf('(');
        int endIndex = lineText.LastIndexOf(')');

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
        {
            return new string[0];
        }
    
        try
        {
            var abilitiesSubstring = lineText.Substring(startIndex + 1, endIndex - startIndex - 1);
 
            return abilitiesSubstring
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();
        }
        catch (Exception)
        {
            return new string[0];
        }
    }
    public int GetNumberAliveMonsters()
    {
        int counter = 0;
        foreach (var monstruo in Monsters)
        {
            if (monstruo != null && monstruo.Attributes.CurrentHp > 0) counter++;
        }

        return counter;
    }
    
    public void RealoadTurns()
    {
        Turns = new List<Turn>();
        for (int i = 0; i < GetNumberUnitsInStartingTeam(); i++)
        {
            Turns.Add(new Turn(TurnType.Full));
        }
    }

    public int GetCurrentFullTurns() =>  Turns.Count(turn => turn != null && turn.Type == TurnType.Full);


    public int GetCurrentBlinkingTurn() =>  Turns.Count(turn => turn != null && turn.Type == TurnType.Blinking);
    
    
    public int GetNumberUnitsInStartingTeam() => StartingTeam.Count(unit => unit != null && unit.Attributes.CurrentHp > 0);
    
    public string Name() =>  Samurai.Name + $" (J{Identifier})";
    
    public void DestroyTurn(TurnType type)
    {
        int index = Turns.FindIndex(turn => turn != null && turn.Type == type);
        Turns.RemoveAt(index);
    }
    
    public Unit WhoAttack() => OrderForActions.Where(x=>x != null && x.Attributes.CurrentHp > 0).ToArray()[OrderAttack];
    
    public Unit[] GetSelectableUnits() => StartingTeam.Where(x => (x != null && x.Attributes.CurrentHp > 0)).ToArray();


    public bool IsMonsterDuplicate(string name)
    {
        for (int i = 0; i< _monsterId; i++)
        {
            if (Monsters[i].Name == name)  return true;
        }
        return false;
    }
    
    public bool SamuraiExist() => Samurai.Name != null;
}