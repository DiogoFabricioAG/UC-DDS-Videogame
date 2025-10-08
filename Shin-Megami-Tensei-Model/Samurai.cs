using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;


public class Samurai : Unit
{
    public override List<ActionType> GetAvailableActions()
    {
        return
        [
            ActionType.Attack,
            ActionType.Shoot,    
            ActionType.Spell,  
            ActionType.Invoke, 
            ActionType.Pass, 
            ActionType.Surrender 
        ];
    }
}