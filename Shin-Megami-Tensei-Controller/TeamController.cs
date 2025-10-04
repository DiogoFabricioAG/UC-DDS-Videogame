using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.UnitsEnums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class TeamController(View view)
{
    private const int TOTAL_MONSTER_IN_TABLE = 3;

    private readonly View _view = view;
    private bool _hasError;
    private static readonly string AbilitiesJsonPath = Path.Combine(AppContext.BaseDirectory, "skills.json");
    private static readonly string MonsterJsonPath = Path.Combine(AppContext.BaseDirectory, "monsters.json");
    private static readonly string SamuraiJsonPath = Path.Combine(AppContext.BaseDirectory, "samurai.json");


    public bool EnterUnits(string[] inputLines, Team team)
    {
        _hasError = false; 

        foreach (var line in inputLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Contains("Samurai"))
            {
                InsertSamuraiIntoTeam(line, team);
            }
            else
            {
                InsertMonsterFromInput(line, team);
            }
        
            if (_hasError) break; 
        }

        return _hasError; 
    }
    
    private void InsertMonsterFromInput(string line, Team team)
    {
        var monster = FromInputGetMonster(line); 

        if (monster == null)
        {
            _hasError = true;
            return;
        }

        InsertMonsterIntoTeam(monster, team);
    }
    private void InsertMonsterIntoTeam(Monster monster, Team team)
    {
        if (team.isMonsterInsertInvalid(monster))
        {
            _hasError = true;
            return;
        }

        team.Monsters[team.MonsterId] = monster;
        team.MonsterId++;

        if (team.MonsterId < TOTAL_MONSTER_IN_TABLE + 1)
        {
            team.Turns.Add(new Turn(TurnType.Full));
        }
    }
    
    private static Monster FromInputGetMonster(string line)
    {
        var name = line.Trim();
        var monster = DataLoader.GetMonstruoByName(name, MonsterJsonPath,AbilitiesJsonPath);
        return monster;
    }
    
    
    private void InsertSamuraiIntoTeam(string line, Team team)
    {
        var (samuraiName, abilityLines) = ExtractAndValidateSamuraiData(line, team);

        if (string.IsNullOrEmpty(samuraiName)) return;

        if (!LoadAndAssignSamurai(samuraiName, team)) return;

        if (!ProcessAndAssignAbilities(abilityLines, team)) return;

        team.AddTurn(TurnType.Full);
    }

    private (string SamuraiName, IEnumerable<string> AbilityLines) ExtractAndValidateSamuraiData(string line, Team team)
    {
        if (team.SamuraiExist())
        {
            _hasError = true;
            return (null, null);
        }
        
        var samuraiName = Team.FromInputGetSamuraiName(line)?.Trim();

        if (string.IsNullOrEmpty(samuraiName))
        {
            return (null, null);
        }

        var abilityLines = team.FromInputGetAbilities(line);
        
        return (samuraiName, abilityLines);
    }

    private bool LoadAndAssignSamurai(string samuraiName, Team team)
    {
        var loadedSamurai = DataLoader.GetSamuraiByName(samuraiName , SamuraiJsonPath);
        
        if (loadedSamurai == null)
        {
            _hasError = true;
            return false;
        }
        
        team.Samurai = loadedSamurai;
        return true;
    }

    private bool ProcessAndAssignAbilities(IEnumerable<string> abilityLines, Team team)
    {
        foreach (var abilityLine in abilityLines)
        {
            var trimmedAbilityName = abilityLine?.Trim();
            if (string.IsNullOrEmpty(trimmedAbilityName)) continue;
            
            var ability = DataLoader.GetAbilityByName(trimmedAbilityName, AbilitiesJsonPath);
            
            var insertState = team.Samurai.validateAbilityInsert(ability);
            
            if (insertState == AbilityInsertState.Unviable)
            {
                _hasError = true;
                return false; 
            }
            
            if (insertState == AbilityInsertState.Correct)
            {
                team.Samurai.AddAbility(ability);
            }
        }
        return true;
    }

    public static void HandleUnitsDestroyed(Team team)
    {
        var unitsDestroyed = team.FindUnitsDestroyed();

        if (unitsDestroyed.Count == 0)
        {
            return;
        }

        foreach (var unitDestroy in unitsDestroyed)
        {
            team.AddingUnitToBackup(unitDestroy);
            team.AddingUnitToDestroyed(unitDestroy);
            
            team.EliminateUnitFromStarter(unitDestroy);
            team.EliminateUnitFromOrderTurn(unitDestroy);   
        }
    }

    public static void GenerateTeamForInitGame(Team team)
    {
        team.InitializeTeam();
    }
    
}