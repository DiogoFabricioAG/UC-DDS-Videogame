using System.Collections;

namespace Shin_Megami_Tensei_Model;

public class Equipo
{
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
                IngresarSamurai(ObtenerNombreSamurai(line));
            }
            else
            {
                IngresarMounstro(ObtenerMounstro(line));
            }

            if (_error)
            {
                return "Archivo de equipos inválido";
            }
        }
        
        
        return ExisteSamurai() ? ":)" : "Archivo de equipos inválido";
    }

    
    public string ObtenerNombreSamurai(string line) => line.Split(']')[1].Trim();
    public Mounstro ObtenerMounstro(string line) => new Mounstro { nombre = line };
    public void IngresarSamurai(string nombreSamurai)  
    {
        if (ExisteSamurai())
        {
            _error  = true;
            return;
        }
        samurai = new Samurai {nombre = nombreSamurai};
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