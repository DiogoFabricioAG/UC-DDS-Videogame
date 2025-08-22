using System.Collections;
using System.IO; // para Path

namespace Shin_Megami_Tensei_Model;

public class Equipo
{

    // CONST ERROR
    private const string ERRORMESSAGE = "Archivo de equipos inválido";
    private const int CANTIDADMAXIMAMONSTRUOS = 7;
    private const int TOTALMONSTERINTABLE = 3;
    private const int MAXUNITSINTABLE = 4;
    public string numero { get; set; } = "0";
    public Samurai samurai { get; set; } = new Samurai();
    public Monstruo[] monstruos = new Monstruo[CANTIDADMAXIMAMONSTRUOS];
    public Turno[] turnos { get; set; } = new Turno[MAXUNITSINTABLE];
    private bool _error;
    private int MonsterID = 0;


    public int getMonsterID() => MonsterID;

    public string IngresarEquipo(string[] inputLines)
    {
        foreach (string line in inputLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Contains("Samurai"))
            {

                IngresarSamurai(line);
            }
            else
            {
                IngresarMounstro(ObtenerMounstro(line));
            }
        }


        return !ExisteSamurai() || _error ? ERRORMESSAGE : "Porque entra por aca?";
    }



    // Devolver Tupla con TIPO | UNIDAD | HABILIDADES 
    public string? ObtenerNombreSamurai(string line) => line.Split(' ')[1];

    public string[] ObtenerHabilidades(string lineText)
    {
        try
        {

            var habilidadesSinFiltrado = lineText.Split(' ').Skip(2).Aggregate((current, next) => current + " " + next);
            ;
            habilidadesSinFiltrado = habilidadesSinFiltrado.Substring(1, habilidadesSinFiltrado.Length - 2);
            var listaHabilidades = habilidadesSinFiltrado.Split(',');
            return listaHabilidades;
        }
        catch (Exception e)
        {
            return new string[0];
        }
    }

    public Monstruo ObtenerMounstro(string line)
    {
        var name = line.Trim();
        var monsterJsonPath = Path.Combine(AppContext.BaseDirectory, "monsters.json");

        var monster = DataLoader.GetMonstruoByName(name, monsterJsonPath);
        return monster;
    }

    public void IngresarSamurai(string line)
    {
        if (ExisteSamurai())
        {
            _error = true;
            return;
        }

        if (ObtenerNombreSamurai(line) == null)
        {
            return;
        }

        var nombreSamurai = ObtenerNombreSamurai(line).Trim();

        var samuraiJsonPath = Path.Combine(AppContext.BaseDirectory, "samurai.json");
        var loadedSamurai = DataLoader.GetSamuraiByName(nombreSamurai, samuraiJsonPath);
        if (loadedSamurai == null)
        {
            _error = true;
            return;
        }

        samurai = loadedSamurai;

        foreach (var habilidad in ObtenerHabilidades(line))
        {
            // trim y comprobar cadena vacía
            var htrim = habilidad?.Trim();
            if (string.IsNullOrEmpty(htrim)) continue;
            _error = !samurai.ingresarHabilidad(new Habilidad(htrim));
            if (_error) break;
        }
        turnos[0] = new Turno(TipoTurno.Full);
    }


    public void IngresarMounstro(Monstruo monstruo)
    {
        // Verificar si el objeto monstruo es nulo
        if (monstruo == null)
        {
            Console.WriteLine("Error de los MONSTRUOS");

            _error = true;
            return;
        }

        if (DuplicadoMonstruo(monstruo.nombre) || MonsterID == CANTIDADMAXIMAMONSTRUOS)
        {
            _error = true;
            return;
        }
        monstruos[MonsterID] = monstruo;
        MonsterID++;
        Console.WriteLine(MonsterID);
        if (MonsterID < TOTALMONSTERINTABLE + 1 )
        {
            turnos[MonsterID] = new Turno(TipoTurno.Full);
        }
    }

    public int AliveMonsters()
    {
        int counter = 0;
        foreach (var monstruo in monstruos)
        {
            if (monstruo != null && monstruo.atributos.hpActual > 0) counter++;
        }

        return counter;
    }

    public int TotalFullTurns() => Math.Min(AliveMonsters(), TOTALMONSTERINTABLE) + 1;

    public void RealoadTurns()
    {
        for (int i = 0; i < TotalFullTurns(); i++)
        {
            turnos[i] = new Turno(TipoTurno.Full);
        }
    }

    public int GetCurrentFullTurns()
    {
        int counter = 0;
        foreach (var turn in turnos)
        {
            if (turn != null && turn.tipo == TipoTurno.Full)
            {
                counter++;
            }
        }

        return counter;
    }

    public int GetCurrentBlinkingTurn()
    {
        int counter = 0;

        foreach (var turn in turnos)
        {
            if (turn != null && turn.tipo == TipoTurno.Blinking)
            {
                counter++;
            }
        }

        return counter;
    }


    // Falta colocar una lista de Interfaces (creo?) que recorra a los unicas unidades que podran participar en el turno
    // del equipo. Luego con eso utilizariamos el metodo de Personaje.selectOptions().

    public String[] CurrentTurnOrder()
    {
        var orderLogs = "Orden:" + ";"; 
        orderLogs += $"1-{samurai.nombre}" ;
        for (int i = 1; i < Math.Min(AliveMonsters() + 1, TOTALMONSTERINTABLE + 1); i++)
            orderLogs += ";" + $"{i + 1}-{monstruos[i - 1].nombre}" ;
        
        return orderLogs.Split(';');
    }

    public String[] CurrentTurnsbyType()
    {
        var turnsLog = $"Full Turns: {GetCurrentFullTurns()}" + ";";
        turnsLog += $"Blinking Turns: {GetCurrentBlinkingTurn()}";
        return turnsLog.Split(';');
    }


public string Name()
    {
        return samurai.nombre + $" (J{numero})";
    }

    private bool DuplicadoMonstruo(string name)
    {
        for (int i = 0; i< MonsterID; i++)
        {
            if (monstruos[i].nombre == name)  return true;
        }
        return false;
    }
    bool ExisteSamurai() => samurai.nombre != null;
    
}