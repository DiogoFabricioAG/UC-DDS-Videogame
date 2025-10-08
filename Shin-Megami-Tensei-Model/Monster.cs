using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;

public class Monster : Unit
{
    public override List<ActionType> GetAvailableActions()
    {
        return
        [
            ActionType.Attack,
            ActionType.Spell,
            ActionType.Invoke,
            ActionType.Pass
        ];
    }
}