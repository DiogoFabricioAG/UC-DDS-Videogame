using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public class TurnController
{
    private const int TurnWastedNullAbility = 2;
    public static (int blinkingTurnLoss, int fullTurnLoss, int blinkingTurnWon) GetTurnWasted(AbilityType abilityType,
        Unit unitAffected, Team currentTeam)
    {
        switch (unitAffected.Affinity.KnowAffinity(abilityType))
        {
            case AffinityType.Weak:
                return !currentTeam.GetCurrentFullTurns().Equals(0) ? 
                    (0, 1, 1) : 
                    (1 , 0, 0);
            case AffinityType.Null:
                var blinkingTurnAmount =
                    currentTeam.GetCurrentBlinkingTurns() >= TurnWastedNullAbility ? TurnWastedNullAbility : currentTeam.GetCurrentBlinkingTurns();
                var fullTurnAmount = 
                    currentTeam.GetCurrentBlinkingTurns() >= TurnWastedNullAbility ? 0 : Math.Min(TurnWastedNullAbility - currentTeam.GetCurrentBlinkingTurns(), currentTeam.GetCurrentFullTurns());
                
                return (blinkingTurnAmount , fullTurnAmount, 0);
            case AffinityType.Repel:
            case AffinityType.Drain:
                return (currentTeam.GetCurrentBlinkingTurns(), currentTeam.GetCurrentFullTurns(), 0);
            default:
                return !currentTeam.GetCurrentBlinkingTurns().Equals(0) ? 
                    (1, 0, 0) : 
                    (0 , 1, 0);
        }
    }
}