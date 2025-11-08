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

    public Ability(TargetType target)
    {
        Target = target;
    }
    public Ability(JsonAbility data)
    {
        if (data == null) return;
        Name = data.name;
        Type = data.type;
        Cost = data.cost;
        Power = data.power;
        Target = data.target;
        Hits = data.hits;
        Effect = data.effect;
    }
    
    public static void ValidateMp(Unit user, Ability ability)
    {
        if (user.Attributes.CurrentMp < ability.Cost)
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
    }
}