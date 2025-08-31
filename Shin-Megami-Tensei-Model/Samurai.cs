using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;

/*
 Samurai
 -------
 Describe los atributos de la clase Samurai
 Implementando la lógica básica de un personaje 
 */
public class Samurai : Unit
{
    
    
    public override List<ActionType> GetAvailableActions()
    {
        var actions = base.GetAvailableActions();
        actions.Insert(1, ActionType.Shoot); 
        actions.Add(ActionType.Surrender); 
        return actions;
    }
}