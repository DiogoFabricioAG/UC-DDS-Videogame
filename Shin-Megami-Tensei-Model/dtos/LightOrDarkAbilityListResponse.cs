using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model.dtos;

public class LightOrDarkAbilityListResponse(List<LightOrDarkState> state,List<AffinityType> affinityType)
{
    public List<LightOrDarkState> States { get;  } = state;
    public List<AffinityType> Affinities { get;  } = affinityType;
}