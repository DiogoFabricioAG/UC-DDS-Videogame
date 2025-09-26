using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.UnitsEnums;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class TeamController
{
    private const int CANTIDADMAXIMAMONSTRUOS = 7;
    private const int TOTALMONSTERINTABLE = 3;

    private readonly View _view;
    private bool _error;
    private static string abilitiesJsonPath = Path.Combine(AppContext.BaseDirectory, "skills.json");
    private static string monsterJsonPath = Path.Combine(AppContext.BaseDirectory, "monsters.json");
    private static string samuraiJsonPath = Path.Combine(AppContext.BaseDirectory, "samurai.json");


    public TeamController(View view)
    {
        _view = view;
    }

    public bool EnterUnits(string[] inputLines, Team team)
    {
        foreach (string line in inputLines)
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
                InsertMonsterIntoTeam(FromInputGetMonster(line), team);
            }
        }


        return !team.SamuraiExist() || _error;
    }
    
    private void InsertMonsterIntoTeam(Monster monster, Team  team)
    {
        if (monster == null)
        {
            _error = true;
            return;
        }

        if (team.IsMonsterDuplicate(monster.Name) || team.MonsterId == CANTIDADMAXIMAMONSTRUOS)
        {
            _error = true;
            return;
        }
        team.Monsters[team.MonsterId] = monster;
        team.MonsterId++;
        if (team.MonsterId < TOTALMONSTERINTABLE + 1 )
        {
            team.Turns.Add(new Turn(TurnType.Full));
        }
    }
    
    private static Monster FromInputGetMonster(string line)
    {
        var name = line.Trim();
        var monster = DataLoader.GetMonstruoByName(name, monsterJsonPath,abilitiesJsonPath);
        return monster;
    }
    
    
    private void InsertSamuraiIntoTeam(string line, Team team)
    {
        if (team.SamuraiExist())
        {
            _error = true;
            return;
        }

        if (Team.FromInputGetSamuraiName(line) == null)
        {
            return;
        }

        var nombreSamurai = Team.FromInputGetSamuraiName(line).Trim();
        var loadedSamurai = DataLoader.GetSamuraiByName(nombreSamurai, samuraiJsonPath);
        if (loadedSamurai == null)
        {
            _error = true;
            return;
        }
        
        team.Samurai = loadedSamurai;

        

        foreach (var habilidad in team.FromInputGetAbilities(line))
        {
            var htrim = habilidad?.Trim();
            if (string.IsNullOrEmpty(htrim)) continue;
            var ability = DataLoader.GetAbilityByName(htrim, abilitiesJsonPath);

            
            AbilityInsertState abilityInsert = team.Samurai.validateAbilityInsert(ability);
            if (abilityInsert == AbilityInsertState.Unviable)
            {
                _error = true;
                break;
            };
            if (abilityInsert == AbilityInsertState.Correct)
            {
                team.Samurai.AddAbility(ability);
            };
        }
        team.Turns.Add(new Turn(TurnType.Full));
    }
}