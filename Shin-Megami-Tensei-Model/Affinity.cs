using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;

public class Affinity
{
    public List<AbilityType> Weak { get; set; }
    public List<AbilityType> Resist { get; set; }
    public List<AbilityType> Null { get; set; }
    public List<AbilityType> Repel { get; set; }
    public List<AbilityType> Drain { get; set; }

    public AffinityType KnowAffinity(Ability ability)
    {
        if (Weak.Contains(ability.Type))
        {
            return AffinityType.Weak;
        }
        if (Resist.Contains(ability.Type))
        {
            return AffinityType.Resist;
        }

        if (Null.Contains(ability.Type))
        {
            return AffinityType.Null;
        }

        if (Repel.Contains(ability.Type))
        {
            return AffinityType.Repel;
        }

        return Drain.Contains(ability.Type) ? AffinityType.Drain : AffinityType.Neutral;
    }
}