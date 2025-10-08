using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model.dtos;

public record TurnWastedContext(AbilityType abilityType, Unit unitAffected, Team currentTeam, bool haveAffinity = true);