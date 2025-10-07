
namespace Shin_Megami_Tensei_Model;

public class Team
{

    private const int MAX_MONSTERS_COUNT = 7;
    private const int TOTAL_MONSTER_IN_TABLE = 3;
    private const int MAX_UNITS_IN_TABLE = 4;

    public int TeamTurnOrder { get; set; }
    public string Identifier { get; set; } = "0";
    public Samurai Samurai { get; set; } = new Samurai();
    public Unit[] Monsters { get; set; } = new Unit[MAX_MONSTERS_COUNT];
    public List<Turn> Turns { get; private set; } = [];

    private List<Unit> BackupTeam { get; set; } = [];
    public List<Unit> DestroyedUnits { get; set; } = [];

    public TeamState State { get; set; } = TeamState.Initialized;

    public int NumAbilitiesCast { get; set; } = 0;

    public Unit[] StartingTeam { get; set; } = new Unit[MAX_UNITS_IN_TABLE];

    public List<Unit> OrderForActions { get; set; } = [];

    private bool _hasError = false;


    public int  MonsterId { get; set; } = 0;


    public int NumberTeam { get; set; }
    
    public void ChangeOrder()
    {
        TeamTurnOrder++;
        if (TeamTurnOrder == GetNumberUnitsInStartingTeam())
        {
            TeamTurnOrder = 0;
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

    public void AddingUnitToBackup(Unit unitDestroy)
    {
        if (!BackupTeam.Contains(unitDestroy))
        {
            BackupTeam.Insert(0,unitDestroy);
        }
    }
    public void AddingUnitToDestroyed(Unit unitDestroy)
    {
        if (!DestroyedUnits.Contains(unitDestroy))
        {
            DestroyedUnits.Add(unitDestroy);
        }
    }
    public void EliminateUnitFromStarter(Unit unitDestroy)
    {
        if (StartingTeam[Array.IndexOf(StartingTeam, unitDestroy)] is Monster)
        {
            StartingTeam[Array.IndexOf(StartingTeam, unitDestroy)] = null;
        }
    }
    public void EliminateUnitFromOrderTurn(Unit unitDestroy)
    {
        
        if (OrderForActions.IndexOf(unitDestroy) != -1)
        {
            OrderForActions[OrderForActions.IndexOf(unitDestroy)] = null;
        }
    }

    public List<Unit> FindUnitsDestroyed()
    {
        return StartingTeam.Where(unit => unit != null).ToArray()
            .Where(unit => unit.Attributes.CurrentHp == 0)
            .Where(unit => OrderForActions.Contains(unit))
            .ToList();
    }
    
    public void AddTurns(TurnType turnType, int nTurns = 1)
    {
        for (var i = 0; i < nTurns; i++)
        {
            Turns.Add(new Turn(turnType));
        }
    }
    public void InitializeTeam()
    {
        SelectStarterTeam();
        SelectBackupTeam();
        GenerateTurnOrder();
    }
    public void ReviveUnit(Unit unitToRevive, Unit reviverUnit)
    {
        
        if (DestroyedUnits.Contains(unitToRevive))
        {
            DestroyedUnits.Remove(unitToRevive);
        }

        if (unitToRevive is not Shin_Megami_Tensei_Model.Samurai) return;
        if (OrderForActions.IndexOf(unitToRevive) == OrderForActions.Count)
        {
            OrderForActions.Add(unitToRevive);
        }
        else
        {
                
            OrderForActions.Insert(OrderForActions.IndexOf(reviverUnit) , unitToRevive);
            TeamTurnOrder++;
        }
    }
    
    private void SelectStarterTeam()
    {
        if (Samurai == null)
        {
            _hasError = true;
            return;
        }
        StartingTeam[0] = Samurai;
        
        for (var i = 0; i < Math.Min(GetNumberAliveMonsters(), TOTAL_MONSTER_IN_TABLE); i++)
        {
            StartingTeam[i + 1] = Monsters[i];
        }
    }

    private void SelectBackupTeam()
    {
        foreach (var monster in Monsters.Where(x => x != null))
        {
            if (!StartingTeam.Contains(monster))
            {
                BackupTeam.Add(monster);
            }
        }
    }

    private void GenerateTurnOrder()
    {
        OrderForActions = StartingTeam.Where(x => x != null && x.Attributes.CurrentHp > 0).OrderByDescending(x => x.Attributes.Speed).ToList();
    }
    
   
    public string[] FromInputGetAbilities(string lineText)
    {
        var startIndex = lineText.IndexOf('(');
        var endIndex = lineText.LastIndexOf(')');

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
        {
            return [];
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
            return [];
        }
    }
    private int GetNumberAliveMonsters()
    {
        return Enumerable.OfType<Unit>(Monsters).Count(monster => monster.Attributes.CurrentHp > 0);
    }
    
    public void ReloadTurns()
    {
        Turns = [];
        for (int i = 0; i < GetNumberUnitsInStartingTeam(); i++)
        {
            Turns.Add(new Turn(TurnType.Full));
        }

        GenerateTurnOrder();
    }

    public int GetCurrentFullTurns() =>  Turns.Count(turn => turn != null && turn.Type == TurnType.Full);


    public int GetCurrentBlinkingTurns() =>  Turns.Count(turn => turn != null && turn.Type == TurnType.Blinking);
    
    
    public int GetNumberUnitsInStartingTeam() => StartingTeam.Count(unit => unit != null && unit.Attributes.CurrentHp > 0);
    
    public string GetName() =>  Samurai.Name + $" (J{Identifier})";
    
    public void RemoveTurns(TurnType type,int nTurns = 1 )
    {
        for (var i = 0; i < nTurns; i++)
        {
            var index = Turns.FindIndex(turn => turn != null && turn.Type == type);
            Turns.RemoveAt(index);
        }
        
    }
    public int GetCancelOptionInvoke(bool showAll = false) => GetMonstersInBackup(showAll).Count + 1;

    public Unit GetUnitInTurn() => OrderForActions.Where(x=>x != null && x.Attributes.CurrentHp > 0).ToArray()[TeamTurnOrder];
    
    public Unit[] GetSelectableUnits(bool showAll = false) => StartingTeam.Where(x => (x != null && (x.Attributes.CurrentHp > 0 || showAll))).ToArray();
    public Unit[] GetDefeatedUnits() => DestroyedUnits.OrderBy(x => Array.IndexOf(Monsters, x)).ToArray();

    public int GetCancelButtonReviveUnits() => DestroyedUnits.Count + 1;

    public int GetCancelButtonReplacebleUnits() => GetReplaceableTeam().Length + 1;
    
    public List<Unit> GetMonstersInBackup(bool showAll = false) => BackupTeam
        .Where(x => x != null && (x.Attributes.CurrentHp > 0 || showAll) && x is Monster)
        .OrderBy(x => Array.IndexOf(Monsters, x)) 
        .ToList();
    public Unit[] GetReplaceableTeam()  => StartingTeam.Where(x =>  x is not Shin_Megami_Tensei_Model.Samurai).ToArray();


    public (Unit replacedUnit, bool wasRevived) ReplaceUnit(int backupIndex, int starterIndex, bool allowRevival)
    {
        var unitToSwapIn = GetUnitFromBackup(backupIndex, allowRevival);
        var unitToSwapOut = StartingTeam[starterIndex];

        UpdateTurnOrderForSwap(unitToSwapIn, unitToSwapOut, starterIndex);


        
        PerformUnitSwap(unitToSwapIn, starterIndex);

        if (allowRevival)
        {
            return HandleRevivalLogic(starterIndex);
        }
        
        return (StartingTeam[starterIndex], false);
    }

    private void UpdateTurnOrderForSwap(Unit unitToSwapIn, Unit unitToSwapOut, int starterIndex)
    {
        if (unitToSwapOut == null) 
        {

            OrderForActions.Insert(OrderForActions.IndexOf(GetUnitInTurn()), unitToSwapIn);
            TeamTurnOrder++;
        }
        else 
        {
            OrderForActions[OrderForActions.IndexOf(unitToSwapOut)] = unitToSwapIn;
        }
    }

    private void PerformUnitSwap(Unit unitToSwapIn, int starterIndex)
    {
        var backupUnitIndex = BackupTeam.IndexOf(unitToSwapIn);
        
        var temp = StartingTeam[starterIndex];
        StartingTeam[starterIndex] = unitToSwapIn;
        BackupTeam[backupUnitIndex] = temp;
        
        BackupTeam = BackupTeam.OrderBy(x => Array.IndexOf(Monsters, x)).ToList();
    }

    private (Unit, bool) HandleRevivalLogic(int starterIndex)
    {
        var swappedInUnit = StartingTeam[starterIndex];
        
        if (swappedInUnit.Attributes.CurrentHp == 0)
        {
            swappedInUnit.Attributes.CurrentHp = swappedInUnit.Attributes.MaxHp;
            
            DestroyedUnits.RemoveAt(DestroyedUnits.IndexOf(swappedInUnit));
            
            return (swappedInUnit, true);
        }
        return (swappedInUnit, false);
    }

    private Unit GetUnitFromBackup(int indexBackup, bool deadUnitsToo) => GetMonstersInBackup(deadUnitsToo)[indexBackup - 1]; 
    public Unit FindTargetUnit(int targetIndex, bool includeDefeated ) => includeDefeated ? 
        GetDefeatedUnits()[targetIndex - 1 ] : 
        GetSelectableUnits()[targetIndex-1];
    
    private bool IsMonsterDuplicate(Monster monster) => Monsters.Contains(monster);
    
    public bool SamuraiExist() => Samurai.Name != null;
    
    
    public int KnowIndexFromUnitInStartingTeam(Unit unit) => Array.IndexOf(StartingTeam, unit);

    public bool IsMonsterInsertInvalid(Monster monster) => IsMonsterDuplicate(monster) || MonsterId == MAX_MONSTERS_COUNT || monster == null;

}