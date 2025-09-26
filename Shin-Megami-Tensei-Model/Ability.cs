namespace Shin_Megami_Tensei_Model;

public class Ability
{
    public Ability(string name, string type, int cost, int power, string target, string hits, string effect)
    {
        Name = name;
        Type = type;
        Cost = cost;
        Power = power;
        Target = target;
        Hits = hits;
        Effect = effect;
    }

    public string Presentation() => $"{Name} MP:{Cost}";
    public string Name { get; set; }

    public string Type { get; set; }

    public int Cost { get; set; }

    public int Power { get; set; }

    public string Target { get; set; }

    public string Hits { get; set; }

    public string Effect { get; set; }


    public Ability(string name)
    {
        Name = name;
    }

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

    public Ability()
    {
        throw new NotImplementedException();
    }
}