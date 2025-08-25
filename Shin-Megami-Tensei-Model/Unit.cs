namespace Shin_Megami_Tensei_Model;

public class Unit
{
    private const double ATTACKCONSTADJUST = 0.0114;
    private const int CANTIDADHABILIDADESMAXIMA = 8;

    private string name;
    public string Name { get => name; set => name = value; }
    private Attributes _attributes;
    public Attributes Attributes { get => _attributes; set => _attributes = value; }
    private Affinity _affinity;
    public Affinity Affinity  { get => _affinity; set => _affinity = value; }

    private Ability[] _ability;
    
    public Ability[] Ability  { get => _ability; set => _ability = value; } 

    public string Status()
    {
        return $"{name} HP:{_attributes.CurrentHp}/{_attributes.MaxHp} MP:{_attributes.CurrentMp}/{_attributes.MaxMp}";
    }
    public virtual string[] SelectOptions() => new string[]
    {
        $"Seleccione una acción para {name}",
        "1: Atacar",
        "2: Usar Habilidad",
        "3: Invocar",
        "4: Pasar Turno"
    };
    
    bool ValidateAbilityInsert(Ability ability)
    {
        bool _error = false;
        for (int i = 0; i < _idHabilidad; i++)
            if (this._ability[i].Name == ability.Name)
            {
                _error = true;
                break;
            }
        return _error;
            
        
    }
    public bool AbilityInsert(Ability ability)
    {
        if (ValidateAbilityInsert(ability) || CANTIDADHABILIDADESMAXIMA == _idHabilidad)
        {
            return false;
        }
        _ability[_idHabilidad] = ability;
        _idHabilidad++;
        return true;
        
    }

    private int _idHabilidad = 0;
    
    protected Unit()
    {
        _ability = new Ability[CANTIDADHABILIDADESMAXIMA];
    }

    public int AttackKinds(Unit unit, ElementType elementType )
    {
        var modifier = elementType == ElementType.Physics ? 54 : 80;
        var statAttack = elementType == ElementType.Physics ? _attributes.StrikeDmg : _attributes.SkillDmg;
        var damageDone = (int)(modifier * statAttack * ATTACKCONSTADJUST);
        unit._attributes.CurrentHp -= damageDone;
        if (unit._attributes.CurrentHp < 0) unit._attributes.CurrentHp = 0;
        return damageDone;
    }
    
}