using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;


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