
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
    public Turn[]? Turns { get; set; } = new Turn[MAXUNITSINTABLE];

    private Unit[] _startingTeams = new Unit[MAXUNITSINTABLE];
    
    private TeamState _state = TeamState.Initialized;
    public TeamState State { get  => _state; set => _state = value; }
    
    public Unit[] StartingTeam
    {
        get => _startingTeams;
        set => _startingTeams = value;
    }

    // Surrender Action
    private bool _surrender = false;
    
    public bool Surrender
    {
        get => _surrender;
        set => _surrender = value;  
    }
    
    private Unit[] orderTeam = new Unit[MAXUNITSINTABLE];

    public Unit[] OrderTeam
    {
        get => orderTeam;
        set => orderTeam = value;
    }
    
    private bool _error;
    private int _monsterId = 0;
    public int  MonsterId { get => _monsterId; set => _monsterId = value; }

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
    
    public static string? FromInputGetSamuraiName(string line) => line.Split(' ')[1];


    public void AnyUnitDestroyed()
    {
        Unit unitDestroy = null;
        // Escapa de Ataques a multiples unidades
        foreach (var unit in StartingTeam.Where(unit => unit != null).ToArray())
        {
            if (unit.Attributes.CurrentHp == 0)
            {
                if (orderTeam.Contains(unit))
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
            if (Array.IndexOf(OrderTeam, unitDestroy) != -1)
                OrderTeam[Array.IndexOf(OrderTeam, unitDestroy)] = null;

        }
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
        
        OrderTeam = StartingTeam.Where(x => x != null).OrderByDescending(x => x.Attributes.Speed).ToArray();
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
        Turns = new Turn[MAXUNITSINTABLE];
        for (int i = 0; i < GetNumberUnitsInStartingTeam(); i++)
        {
            Turns[i] = new Turn(TurnType.Full);
        }
    }

    public int GetCurrentFullTurns()
    {
        int counter = 0;
        foreach (var turn in Turns)
        {
            if (turn != null && turn.Type == TurnType.Full)
            {
                counter++;
            }
        }

        return counter;
    }

    public int GetCurrentBlinkingTurn()
    {
        int counter = 0;

        foreach (var turn in Turns)
        {
            if (turn != null && turn.Type == TurnType.Blinking)
            {
                counter++;
            }
        }

        return counter;
    }
    
    public int GetNumberUnitsInStartingTeam() => StartingTeam.Count(unit => unit != null && unit.Attributes.CurrentHp > 0);
    
    public string Name() =>  Samurai.Name + $" (J{Identifier})";
    
    public void DestroyTurn(TurnType type)
    {
        int iTurn = -1;
        for (int i = 0; i < MAXUNITSINTABLE; i++)
        {
            if (Turns[i] != null &&  Turns[i].Type == type)
            {
                iTurn = i;
                break;
            }
        }
        if (iTurn != -1) Turns[iTurn] = null;
    }
    
    public Unit WhoAttack() => OrderTeam.Where(x=>x != null && x.Attributes.CurrentHp > 0).ToArray()[OrderAttack];
    
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