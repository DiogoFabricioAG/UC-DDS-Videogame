using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei;

public abstract class TurnController
{
    private const int TurnWastedNullAbility = 2;

    public static (int blinkingLoss, int fullLoss, int blinkingWon) GetTurnWasted(
        TurnWastedContext ctx)
    {
        if (!ctx.haveAffinity) return GetLossForNoAffinity(ctx.currentTeam);
        var affinity = ctx.unitAffected.Affinity.GetAffinity(ctx.abilityType);

        return affinity switch
        {
            AffinityType.Resist or AffinityType.Neutral => GetLossForStandardAffinity(ctx.currentTeam),
            AffinityType.Weak => GetLossForWeakAffinity(ctx.currentTeam),
            AffinityType.Null => GetLossForNullAffinity(ctx.currentTeam),
            AffinityType.Repel or AffinityType.Drain => GetLossForReflectingAffinity(ctx.currentTeam),
            _ => GetLossForWeakAffinity(ctx.currentTeam)
        };
    }

    public static (int blinkingLoss, int fullLoss, int blinkingWon) GetTurnWastedByTeam(Ability ability, Team team, Team teamOnTurn, bool containsMissAttacks)
    {
        
        AffinityType affinity = Affinity.GetPrioritizeAffinity(team.StartingTeam, ability);
        if (containsMissAttacks && affinity is not (AffinityType.Drain or AffinityType.Repel or AffinityType.Null))
        {
            Console.WriteLine("ERRRORR");
            return GetLossForMissAffinity(teamOnTurn);
        }
        return affinity switch
        {
            AffinityType.Resist or AffinityType.Neutral => GetLossForStandardAffinity(teamOnTurn),
            AffinityType.Weak => GetLossForWeakAffinity(teamOnTurn),
            AffinityType.Null => GetLossForNullAffinity(teamOnTurn),
            AffinityType.Repel or AffinityType.Drain => GetLossForReflectingAffinity(teamOnTurn),
            _ => GetLossForWeakAffinity(teamOnTurn)
        };
    }

    private static (int blinkingLoss, int fullLoss, int blinkingWon) GetLossForStandardAffinity(Team team)
    {
        return team.GetCurrentBlinkingTurns() > 0 ? (1, 0, 0) : (0, 1, 0);
    }

    private static (int blinkingLoss, int fullLoss, int blinkingWon) GetLossForWeakAffinity(Team team)
    {
        return team.GetCurrentFullTurns() > 0 ? (0, 1, 1) : (1, 0, 0);
    }
    
    private static (int blinkingLoss, int fullLoss, int blinkingWon) GetLossForMissAffinity(Team team)
    {
        return team.GetCurrentBlinkingTurns() > 0 ? (1, 0, 0) : (0, 1, 0);
    }
    private static (int blinkingLoss, int fullLoss, int blinkingWon) GetLossForReflectingAffinity(Team team)
    {
        return (team.GetCurrentBlinkingTurns(), team.GetCurrentFullTurns(), 0);
    }
    
    private static (int blinkingLoss, int fullLoss, int blinkingWon) GetLossForNoAffinity(Team team)
    {
        return team.GetCurrentBlinkingTurns() > 0 ? (1, 0, 0) : (0, 1, 1);
    }

    private static (int blinkingLoss, int fullLoss, int blinkingWon) GetLossForNullAffinity(Team team)
    {
        var currentBlinking = team.GetCurrentBlinkingTurns();
        var currentFull = team.GetCurrentFullTurns();
        
        var blinkingLoss = Math.Min(currentBlinking, TurnWastedNullAbility);
        
        var remainingLoss = TurnWastedNullAbility - blinkingLoss;
        
        var fullLoss = Math.Min(currentFull, remainingLoss);

        return (blinkingLoss, fullLoss, 0);
    }
    
    public static void DestroyAndAddTurns(Team team, TurnContext turnContext)
    {
        team.RemoveTurns(TurnType.Blinking, turnContext.blinkingTurnLoss);
        team.RemoveTurns(TurnType.Full,turnContext.fullTurnLoss);
        team.AddTurns(TurnType.Blinking, turnContext.blinkingTurnWon);
    }
}