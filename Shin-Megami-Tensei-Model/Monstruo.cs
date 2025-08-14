namespace Shin_Megami_Tensei_Model;

public class Monstruo : Personaje
{
    public string nombre { get; set; }
    public Atributos atributos { get; set; }
    public Afinidad afinidad { get; set; }
    public Habilidad[] habilidades { get; set; }
    public bool ingresarHabilidad(Habilidad habilidad)
    {
        if (!habilidades.Contains(habilidad))
        {
            habilidades[0] = habilidad;
            return true;
        }
        return false;
    }
}