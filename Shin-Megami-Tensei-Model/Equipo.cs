using System.Collections;

namespace Shin_Megami_Tensei_Model;

public class Equipo
{
    
    // CONST ERROR
    private const string ERRORMESSAGE = "Archivo de equipos inválido";
    
    public string nombre { get; set; }
    public Samurai samurai { get; set; }
    public Monstruo[] monstruos = new Monstruo[7];
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

            if (_error)
            {
                return ERRORMESSAGE;
            }
        }
        
        
        return ExisteSamurai() ? ":)" : ERRORMESSAGE;
    }

    
    
    // Devolver Tupla con TIPO | UNIDAD | HABILIDADES 
    public string? ObtenerNombreSamurai(string line) => line.Split(' ')[1];

    public string[] ObtenerHabilidades(string lineText)
    {
        try
        {
            var habilidadesSinFiltrado = lineText.Split(' ')[2];
            habilidadesSinFiltrado = habilidadesSinFiltrado.Substring(1, habilidadesSinFiltrado.Length - 2);
            var listaHabilidades = habilidadesSinFiltrado.Split(',');
            return listaHabilidades;
        }
        catch (Exception e)
        {
            Console.WriteLine("Erroresss");
            return new string[0];
        }
    }
    public Monstruo ObtenerMounstro(string line) => new Monstruo { nombre = line };
    public void IngresarSamurai(string line)  
    {
        if (ExisteSamurai())
        {
            _error  = true;
            return;
        }
        samurai = new Samurai {nombre = ObtenerNombreSamurai(line)};
        
        foreach (var habilidad in ObtenerHabilidades(line))
        {
            Console.WriteLine(habilidad);
            _error  = !samurai.ingresarHabilidad(new Habilidad(habilidad));
            if (_error)
            {
                break;
            }
        }
    }

    public void IngresarMounstro(Monstruo monstruo)
    {
        if (DisponibleMounstro() || DuplicadoMonstruo(monstruo.nombre))
        {
            _error  = true;
            return;
        }
        monstruos[MonsterID] = monstruo;
        MonsterID++;
    }

    bool DuplicadoMonstruo(string name)
    {
        for (int i = 0; i< MonsterID; i++)
        {
            if (monstruos[i].nombre == name)  return true;
        }
        return false;
    }
    bool DisponibleMounstro() => monstruos[monstruos.Length - 1] != null;
    bool ExisteSamurai() => samurai != null;
    
}