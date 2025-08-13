namespace Shin_Megami_Tensei_Model;

public class Partida
{
    public Equipo equipo1 { get; set; }
    public Equipo equipo2 { get; set; }

    // Constructor para inicializar los equipos
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
        var result2 = equipo2.IngresarEquipo(alineacionE2);

        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }

        if (!string.IsNullOrEmpty(result2))
        {
            return result2;
        }
        return "helo";
    }
}