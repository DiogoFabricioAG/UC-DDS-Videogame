using System.Collections;

namespace Shin_Megami_Tensei_Model;




public class Partida
{
    private const string SEPARATOR = "----------------------------------------";
    const int PLAYERS = 2;
    public Equipo equipo1 { get; set; }
    public Equipo equipo2 { get; set; }

    public ArrayList Log = new ArrayList();
    public Partida()
    {
        equipo1 = new Equipo();
        equipo2 = new Equipo();
    }
    
    public string CreacionEquipo(string[] lines)
    {
        var position1 = 0;
        int position2 = 1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Player 2"))
            {
                position2 = i;
                break;
            }
        }
        string[] alineacionE1 = lines.Skip(1).Take(position2-1).ToArray();
        string[] alineacionE2 = lines.Skip(position2+1).Take(lines.Length - position2 - 1).ToArray();
        var result = equipo1.IngresarEquipo(alineacionE1);
        equipo1.numero = "1";
        var result2 = equipo2.IngresarEquipo(alineacionE2);
        equipo2.numero = "2";


        if (result == "Archivo de equipos inválido")
        {
            return result;
        }
        if (result2 == "Archivo de equipos inválido")
        {
            return result2;
        }
        return SEPARATOR;
    }

    public void InitialDialog()
    {
        Log.Add($"Ronda de {equipo1.Name()}");
        Log.Add(SEPARATOR);

    }
}