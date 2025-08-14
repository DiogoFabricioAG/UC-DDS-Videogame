using System.Collections;

namespace Shin_Megami_Tensei_Model;

public class Equipo
{
    
    // CONST ERROR
    private const string ERRORMESSAGE = "Archivo de equipos inválido";
    
    public string nombre { get; set; }
    public Samurai samurai { get; set; }
    public Mounstro[] mounstros = new Mounstro[7];
    public List<Turno> turnos {get; set; }
    private bool _error;
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
    public Mounstro ObtenerMounstro(string line) => new Mounstro { nombre = line };
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

    public void IngresarMounstro(Mounstro mounstro)
    {
        if (DisponibleMounstro())
        {
            _error  = true;
            return;
        }

        for (int i = 0; i < mounstros.Length; i++)
        {
            if (mounstros[i] == null)
            {
                mounstros[i] = mounstro;
                break;
            }
        }
    }
    
    bool DisponibleMounstro() => mounstros[mounstros.Length - 1] != null;
    bool ExisteSamurai() => samurai != null;
    
}