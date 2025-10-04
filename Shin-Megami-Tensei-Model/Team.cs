
namespace Shin_Megami_Tensei_Model;

public class Team
{

    private const int CANTIDADMAXIMAMONSTRUOS = 7;
    private const int TOTALMONSTERINTABLE = 3;
    private const int MAXUNITSINTABLE = 4;

    public int OrderAttack { get; set; }
    public string Identifier { get; set; } = "0";
    public Samurai Samurai { get; set; } = new Samurai();
    public Unit[] Monsters { get; set; } = new Unit[CANTIDADMAXIMAMONSTRUOS];
    public List<Turn> Turns { get; set; } = [];

    public List<Unit> BackupTeam { get; set; } = new List<Unit>();
    public List<Unit> DestroyedUnits { get; set; } = [];

    private Unit[] _startingTeams = new Unit[MAXUNITSINTABLE];
    
    private TeamState _state = TeamState.Initialized;
    public TeamState State { get  => _state; set => _state = value; }

    public int NumAbilitiesCast { get; set; } = 0;

    public Unit[] StartingTeam
    {
        get => _startingTeams;
        set => _startingTeams = value;
    }

    private List<Unit> _orderForActions = [];

    public List<Unit> OrderForActions
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

    public int GetCancelOptionAbilities() => GetUnitInTurn().GetTotalAbilities().Length + 1;
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
                if (OrderForActions.Contains(unit))
                {
                    unitDestroy = unit;
                    break;
                }
            }
        }

        if (unitDestroy != null)
        {
            if (!BackupTeam.Contains(unitDestroy))
            {
                BackupTeam.Insert(0,unitDestroy);
            }

            if (!DestroyedUnits.Contains(unitDestroy))
            {
                DestroyedUnits.Add(unitDestroy);
            }
            if (StartingTeam[Array.IndexOf(StartingTeam, unitDestroy)] is Monster)
            {
                StartingTeam[Array.IndexOf(StartingTeam, unitDestroy)] = null;
            }
            if (OrderForActions.IndexOf(unitDestroy) != -1)
                OrderForActions[OrderForActions.IndexOf(unitDestroy)] = null;
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

        OrderTeam();

        foreach (var monster in Monsters.Where(x => x != null))
        {
            if (!StartingTeam.Contains(monster))
            {
                BackupTeam.Add(monster);
            }
        }
    }

    public void OrderTeam()
    {
        OrderForActions = StartingTeam.Where(x => x != null).OrderByDescending(x => x.Attributes.Speed).ToList();
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
    private int GetNumberAliveMonsters()
    {
        int counter = 0;
        foreach (var monstruo in Monsters)
        {
            if (monstruo != null && monstruo.Attributes.CurrentHp > 0) counter++;
        }

        return counter;
    }
    
    public void ReloadTurns()
    {
        Turns = new List<Turn>();
        for (int i = 0; i < GetNumberUnitsInStartingTeam(); i++)
        {
            Turns.Add(new Turn(TurnType.Full));
        }

        OrderTeam();
    }

    public int GetCurrentFullTurns() =>  Turns.Count(turn => turn != null && turn.Type == TurnType.Full);


    public int GetCurrentBlinkingTurns() =>  Turns.Count(turn => turn != null && turn.Type == TurnType.Blinking);
    
    
    public int GetNumberUnitsInStartingTeam() => StartingTeam.Count(unit => unit != null && unit.Attributes.CurrentHp > 0);
    
    public string Name() =>  Samurai.Name + $" (J{Identifier})";
    
    public void DestroyTurn(TurnType type)
    {
        var index = Turns.FindIndex(turn => turn != null && turn.Type == type);
        Turns.RemoveAt(index);
    }
    public int GetCancelOptionInvoke(bool showAll = false) => GetMonstersInBackup(showAll).Count + 1;

    public Unit GetUnitInTurn() => OrderForActions.Where(x=>x != null && x.Attributes.CurrentHp > 0).ToArray()[OrderAttack];
    
    public Unit[] GetSelectableUnits() => StartingTeam.Where(x => (x != null && x.Attributes.CurrentHp > 0)).ToArray();
    public Unit[] GetSelectableUnits(bool showAll = false) => StartingTeam.Where(x => (x != null && (x.Attributes.CurrentHp > 0 || showAll))).ToArray();
    public Unit[] GetDefeatedUnits() => DestroyedUnits.OrderBy(x => Array.IndexOf(Monsters, x)).ToArray();

    public int GetCancelButtonReviveUnits() => DestroyedUnits.Count + 1;

    public int GetCancelButtonReplacebleUnits() => GetReplaceableTeam().Length + 1;
    
    public List<Unit> GetMonstersInBackup(bool showAll = false) => BackupTeam
        .Where(x => x != null && (x.Attributes.CurrentHp > 0 || showAll) && x is Monster)
        .OrderBy(x => Array.IndexOf(Monsters, x)) 
        .ToList();
    public Unit[] GetReplaceableTeam()  => StartingTeam.Where(x =>  x is not Shin_Megami_Tensei_Model.Samurai).ToArray();

    public (Unit, bool) ReplaceUnit(int indexBackup, int indexStarter, bool deadUnitsToo)
    {
        var backupUnitIndex = BackupTeam.IndexOf(GetMonstersInBackup(deadUnitsToo)[indexBackup - 1]);
        
        if (StartingTeam[indexStarter] == null)
        {

            OrderForActions.Insert(OrderForActions.IndexOf(GetUnitInTurn()), BackupTeam[backupUnitIndex]);
            OrderAttack++;

        }
        else
        {
            OrderForActions[OrderForActions.IndexOf(StartingTeam[indexStarter])] = BackupTeam[backupUnitIndex];
            // OJITO A LO QUE ES EL DESPROPOSITO DE BACKUPTEAM
        }

        
        (StartingTeam[indexStarter], BackupTeam[backupUnitIndex]) =
            (BackupTeam[backupUnitIndex], StartingTeam[indexStarter]);
        BackupTeam = BackupTeam.OrderBy(x => Array.IndexOf(Monsters, x)).ToList();

        if (deadUnitsToo && StartingTeam[indexStarter].Attributes.CurrentHp == 0)
        {
            StartingTeam[indexStarter].Attributes.CurrentHp = StartingTeam[indexStarter].Attributes.MaxHp;
            DestroyedUnits.RemoveAt(DestroyedUnits.IndexOf(StartingTeam[indexStarter]));
            return (StartingTeam[indexStarter], true);
        }
        return (StartingTeam[indexStarter], false);
    }
    
    
    public bool IsMonsterDuplicate(string name)
    {
        for (int i = 0; i< _monsterId; i++)
        {
            if (Monsters[i].Name == name)  return true;
        }
        return false;
    }
    
    public bool SamuraiExist() => Samurai.Name != null;
    
    
    public int KnowIndexFromUnitInStartingTeam(Unit unit) => Array.IndexOf(StartingTeam, unit);

}