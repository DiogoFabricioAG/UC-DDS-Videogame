namespace Shin_Megami_Tensei_Model;

public class Monstruo : Personaje
{
    public string nombre { get; set; }
    public Atributos atributos { get; set; }
    public Afinidad afinidad { get; set; }
    public Habilidad[] habilidades { get; set; }

    public string[] selectOptions() => new string[]
    {
        $"Seleccione una acción para {nombre}",
        "1: Atacar",
        "2: Usar Habilidad",
        "3: Invocar",
        "4: Pasar Turno"
    };

    public bool ingresarHabilidad(Habilidad habilidad)
    {
        if (!habilidades.Contains(habilidad))
        {
            habilidades[0] = habilidad;
            return true;
        }
        return false;
    }
    
    public string Status()
    {
        return $"{nombre} HP:{atributos.hpActual}/{atributos.hpMaximo} MP:{atributos.mpActual}/{atributos.mpMaximo}";
    }
}