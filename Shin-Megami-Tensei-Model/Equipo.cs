using System.Collections;

namespace Shin_Megami_Tensei_Model;

public class Equipo
{
    private string nombre;
    private Samurai samurai;
    private Mounstro[] mounstros = new Mounstro[7];
    private List<Turno> turnos;
}