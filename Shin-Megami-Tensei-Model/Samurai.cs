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
}