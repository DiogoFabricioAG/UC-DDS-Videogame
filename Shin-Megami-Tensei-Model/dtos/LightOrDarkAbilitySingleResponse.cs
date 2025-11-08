using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model.dtos;

public class LightOrDarkAbilitySingleResponse(LightOrDarkState state, AffinityType affinityType)
{
    public LightOrDarkState State { get;  } = state;
    public AffinityType Affinity { get;  } = affinityType;
}