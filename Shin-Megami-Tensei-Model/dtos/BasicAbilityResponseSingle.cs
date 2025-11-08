namespace Shin_Megami_Tensei_Model.dtos;

public class BasicAbilityResponseSingle (int valueAmount, AffinityType affinityType, int numberHits)
{
    public int ValueAmount { get; set; } = valueAmount;
    public AffinityType AffinityType { get; set; } = affinityType;
    public int NumberHits { get; set; } = numberHits;

}