namespace Shin_Megami_Tensei_Model.dtos;

public class BasicAbilityResponseList(List<int> damageDone, List<AffinityType> affinityTypes, List<int> numberHits)
{
    public List<int> DamageDone { get; set; } = damageDone;
    public List<AffinityType> AffinityTypes { get; set; } = affinityTypes;
    public List<int> NumberHits { get; set; } = numberHits;

    public BasicAbilityResponseList(List<int> damageDone, List<AffinityType> affinityTypes) : this(damageDone, affinityTypes, null)
    {
    }
}