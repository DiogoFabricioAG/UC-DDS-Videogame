using System.Collections;
using System.IO; // para Path

namespace Shin_Megami_Tensei_Model;

public class Team
{

    private const string ERRORMESSAGE = "Archivo de equipos inválido";
    private const int CANTIDADMAXIMAMONSTRUOS = 7;
    private const int TOTALMONSTERINTABLE = 3;
    private const int MAXUNITSINTABLE = 4;

    private int orderAttack = 0;
    public string Identifier { get; set; } = "0";
    public Samurai Samurai { get; set; } = new Samurai();
    public Monster[] Monsters { get; set; }  = new Monster[CANTIDADMAXIMAMONSTRUOS];
    public Turn[] Turns { get; set; } = new Turn[MAXUNITSINTABLE];
    
    private Unit[] _startingTeams = new Unit[MAXUNITSINTABLE];
    public Unit[] StartingTeam { get => _startingTeams; set => _startingTeams = value; }

    private bool _error;
    private int _monsterId = 0;
    public int  MonsterId { get => _monsterId; set => _monsterId = value; }

    public void ChangeOrderAttack()
    {
        orderAttack++;
        if (orderAttack == GetNumberUnitsInStartingTeam())
            orderAttack = 0;
    }


    public string EnterUnits(string[] inputLines)
    {
        foreach (string line in inputLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Contains("Samurai"))
            {

                FromInputInsertSamurai(line);
            }
            else
            {
                InsertMonsterIntoTeam(FromInputGetMonster(line));
            }
        }


        return !SamuraiExist() || _error ? ERRORMESSAGE : "Porque entra por aca?";
    }



    private static string? FromInputGetSamuraiName(string line) => line.Split(' ')[1];

    public void SelectStarterTeam()
    {
        if (Samurai == null)
        {
            _error = true;
            return;
        }
        StartingTeam[0] = Samurai;
        
        for (int i = 0; i < GetTotalFullTurns()-1; i++)
        {
            StartingTeam[i + 1] = Monsters[i];
        }
        StartingTeam = StartingTeam.Where(x => x != null).OrderByDescending(x => x.Attributes.Speed).ToArray();
    }
    private string[] FromInputGetAbilities(string lineText)
    
    {
        try
        {
            var habilidadesSinFiltrado = lineText.Split(' ').Skip(2).Aggregate((current, next) => current + " " + next);
            habilidadesSinFiltrado = habilidadesSinFiltrado.Substring(1, habilidadesSinFiltrado.Length - 2);
            var listaHabilidades = habilidadesSinFiltrado.Split(',');
            return listaHabilidades;
        }
        catch (Exception e)
        {
            return new string[0];
        }
    }

    private static Monster FromInputGetMonster(string line)
    {
        var name = line.Trim();
        var monsterJsonPath = Path.Combine(AppContext.BaseDirectory, "monsters.json");

        var monster = DataLoader.GetMonstruoByName(name, monsterJsonPath);
        return monster;
    }

    private void FromInputInsertSamurai(string line)
    {
        if (SamuraiExist())
        {
            _error = true;
            return;
        }

        if (FromInputGetSamuraiName(line) == null)
        {
            return;
        }

        var nombreSamurai = FromInputGetSamuraiName(line).Trim();

        var samuraiJsonPath = Path.Combine(AppContext.BaseDirectory, "samurai.json");
        var loadedSamurai = DataLoader.GetSamuraiByName(nombreSamurai, samuraiJsonPath);
        if (loadedSamurai == null)
        {
            _error = true;
            return;
        }

        Samurai = loadedSamurai;

        foreach (var habilidad in FromInputGetAbilities(line))
        {
            // trim y comprobar cadena vacía
            var htrim = habilidad?.Trim();
            if (string.IsNullOrEmpty(htrim)) continue;
            _error = !Samurai.AbilityInsert(new Ability(htrim));
            if (_error) break;
        }
        Turns[0] = new Turn(TurnType.Full);
    }


    private void InsertMonsterIntoTeam(Monster monster)
    {
        // Verificar si el objeto monstruo es nulo
        if (monster == null)
        {

            _error = true;
            return;
        }

        if (IsMonsterDuplicate(monster.Name) || _monsterId == CANTIDADMAXIMAMONSTRUOS)
        {
            _error = true;
            return;
        }
        Monsters[_monsterId] = monster;
        _monsterId++;
        if (_monsterId < TOTALMONSTERINTABLE + 1 )
        {
            Turns[_monsterId] = new Turn(TurnType.Full);
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

    public int GetTotalFullTurns() => Math.Min(GetNumberAliveMonsters(), TOTALMONSTERINTABLE) + 1;

    public void RealoadTurns()
    {
        for (int i = 0; i < GetTotalFullTurns(); i++)
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


    // Falta colocar una lista de Interfaces (creo?) que recorra a los unicas unidades que podran participar en el turno
    // del equipo. Luego con eso utilizariamos el metodo de Personaje.selectOptions().

    public int GetNumberUnitsInStartingTeam() => StartingTeam.Where(x => x != null).Count();
    public String[] CurrentTurnOrder()
    {
        var orderLogs = "Orden:"; 
        for (int i = 0; i < GetNumberUnitsInStartingTeam(); i++)
            orderLogs += ";" + $"{i + 1}-{StartingTeam[i].Name}" ;
        
        return orderLogs.Split(';');
    }

    public String[] CurrentTurnsbyType()
    {
        var turnsLog = $"Full Turns: {GetCurrentFullTurns()}" + ";";
        turnsLog += $"Blinking Turns: {GetCurrentBlinkingTurn()}";
        return turnsLog.Split(';');
    }


    public Unit WhoAttack() => StartingTeam[orderAttack];
    public string Name()
    {
        return Samurai.Name + $" (J{Identifier})";
    }

    private bool IsMonsterDuplicate(string name)
    {
        for (int i = 0; i< _monsterId; i++)
        {
            if (Monsters[i].Name == name)  return true;
        }
        return false;
    }
    bool SamuraiExist() => Samurai.Name != null;
    
}