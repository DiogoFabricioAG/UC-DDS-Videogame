namespace Shin_Megami_Tensei_Model.dtos;

public class DrainResponse(int values, AffinityType affinityType)
{
    public int Values { get; set; } = values;
    public AffinityType AffinityType { get; set; } = affinityType;
}