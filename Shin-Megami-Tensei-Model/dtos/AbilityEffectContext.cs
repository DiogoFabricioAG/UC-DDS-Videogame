namespace Shin_Megami_Tensei_Model.dtos;

public record AbilityEffectContext(Unit user, Unit target, Ability ability, 
    int damageDone, AffinityType affinityType, int numberHits);