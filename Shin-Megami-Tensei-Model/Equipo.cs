using System.Collections;
using System.IO; // para Path

namespace Shin_Megami_Tensei_Model;

public class Equipo
{
    
    // CONST ERROR
    private const string ERRORMESSAGE = "Archivo de equipos inválido";
    private const int CANTIDADMAXIMAMONSTRUOS = 7;

    public string numero { get; set; } = "0";
    public Samurai samurai { get; set; } =  new Samurai(); 
    public Monstruo[] monstruos = new Monstruo[CANTIDADMAXIMAMONSTRUOS];
    public List<Turno> turnos {get; set; }
    private bool _error;
    private int MonsterID = 0;
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
        

        return !ExisteSamurai() || _error ? ERRORMESSAGE: "Porque entra por aca?";
    }

    
    
    // Devolver Tupla con TIPO | UNIDAD | HABILIDADES 
    public string? ObtenerNombreSamurai(string line) => line.Split(' ')[1];

    public string[] ObtenerHabilidades(string lineText)
    {
        try
        {
            
            var habilidadesSinFiltrado = lineText.Split(' ').Skip(2).Aggregate((current, next) => current + " " + next);;
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
        return monster ;
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
            _error  = true;
            return;
        }
    
        monstruos[MonsterID] = monstruo;
        MonsterID++;
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