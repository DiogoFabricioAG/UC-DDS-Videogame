namespace Shin_Megami_Tensei_Model;

/*
 Samurai
 -------
 Describe los atributos de la clase Samurai
 Implementando la lógica básica de un personaje 
 */
public class Samurai : Personaje
{
    // Implementa la interfaz Personaje


    public string nombre { get; set; }
    public Atributos atributos { get; set; }
    public Afinidad afinidad { get; set; }
    public Habilidad[] habilidades { get; set; }

    public Samurai()
    {
        habilidades = new Habilidad[8];
    }

    private int idHabilidad = 0;
    public bool ingresarHabilidad(Habilidad habilidad)
    {
        if (!validarIngresoHabilidad(habilidad))
        {
            habilidades[idHabilidad] = habilidad;
            idHabilidad++;
            return true;
        }
        return false;
    }

    bool validarIngresoHabilidad(Habilidad habilidad)
    {
        bool _error = false;
        for (int i = 0; i < idHabilidad; i++)
            if (habilidades[i].name == habilidad.name)
            {
                _error = true;
                break;
            }
        return _error;
            
        
    }
}