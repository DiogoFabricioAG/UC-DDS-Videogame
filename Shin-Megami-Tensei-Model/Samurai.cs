namespace Shin_Megami_Tensei_Model;

/*
 Samurai
 -------
 Describe los atributos de la clase Samurai
 Implementando la lógica básica de un personaje 
 */
public class Samurai : Unit
{
    public override string[] SelectOptions() => new string[] 
        { $"Seleccione una acción para {Name}", 
            "1: Atacar",
            "2: Disparar",
            "3: Usar Habilidad",
            "4: Invocar",
            "5: Pasar Turno",
            "6: Rendirse"
        };
}