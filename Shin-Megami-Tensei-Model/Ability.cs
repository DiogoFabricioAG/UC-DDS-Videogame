namespace Shin_Megami_Tensei_Model;

public class Ability
{
    private string _name;

    public string Presentation() => $"{Name} MP:{_cost}";
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    private string _type;

    public string Type
    {
        get => _type;
        set => _type = value;
    }

    private int _cost;

    public int Cost
    {
        get => _cost;
        set => _cost = value;
    }

    private int _power;

    public int Power
    {
        get => _power;
        set => _power = value;
    }

    private string _target;

    public string Target
    {
        get => _target;
        set => _target = value;
    }

    private string _hits;

    public string Hits
    {
        get => _hits;
        set => _hits = value;
    }

    private string _effect;

    public string Effect
    {
        get => _effect;
        set => _effect = value;
    }


    public Ability(string name)
    {
        _name = name;
    }

    public Ability(JsonAbility a)
    {
        if (a == null) return;
        _name = a.name;
        _type = a.type;
        _cost = a.cost;
        _power = a.power;
        _target = a.target;
        _hits = a.hits;
        _effect = a.effect;
    }
    
}