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
    
    public TeamController(View view)
    {
        _view = view;
    }

    // Fijate para el Controller
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

                FromInputInsertSamurai(line, team);
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
        // Verificar si el objeto monstruo es nulo
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
            team.Turns[team.MonsterId] = new Turn(TurnType.Full);
        }
    }
    
    // Para el Controller
    private static Monster FromInputGetMonster(string line)
    {
        var name = line.Trim();
        var monsterJsonPath = Path.Combine(AppContext.BaseDirectory, "monsters.json");

        var monster = DataLoader.GetMonstruoByName(name, monsterJsonPath);
        return monster;
    }
    
    
    // Para el Controller
    private void FromInputInsertSamurai(string line, Team team)
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
        var samuraiJsonPath = Path.Combine(AppContext.BaseDirectory, "samurai.json");
        var loadedSamurai = DataLoader.GetSamuraiByName(nombreSamurai, samuraiJsonPath);
        if (loadedSamurai == null)
        {
            _error = true;
            return;
        }
        
        team.Samurai = loadedSamurai;

        var abilitiesJsonPath = Path.Combine(AppContext.BaseDirectory, "skills.json");

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
        team.Turns[0] = new Turn(TurnType.Full);
    }
}