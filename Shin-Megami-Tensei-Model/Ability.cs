using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model;

public class Ability
{
    public string GetPresentation() => $"{Name} MP:{Cost}";
    public string Name { get; set; }

    public AbilityType Type { get; set; }

    public int Cost { get; set; }

    public int Power { get; set; }

    public TargetType Target { get; set; }

    public string Hits { get; set; }

    public string Effect { get; set; }
    
    public Ability(JsonAbility a)
    {
        if (a == null) return;
        Name = a.name;
        Type = a.type;
        Cost = a.cost;
        Power = a.power;
        Target = a.target;
        Hits = a.hits;
        Effect = a.effect;
    }
}