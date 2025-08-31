using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_Model.UnitsEnums;

namespace Shin_Megami_Tensei_Model;

public abstract class Unit
{
    private const double ATTACK_CONST_JUST = 0.0114;
    private const int MAX_AMOUNT_ABILITIES = 8;
    private const int MODIFIER_PHYSICS = 54;
    private const int MODIFIER_GUN = 80;
    
    

    public int IdAbility
    {
        get;
        set;
    }
    public string Name { get; set; }
    public Attributes Attributes { get; set ; }
    public Affinity Affinity  { get ; set ; }
    

    
    public Ability[] Ability  { get ; set ; } 

    public string Status()
    {
        return $"{Name} HP:{Attributes.CurrentHp}/{Attributes.MaxHp} MP:{Attributes.CurrentMp}/{Attributes.MaxMp}";
    }
    
    public virtual List<ActionType> GetAvailableActions()
    {
        return new List<ActionType>
        {
            ActionType.Attack,
            ActionType.Spell,
            ActionType.Invoke,
            ActionType.Pass
        };
    }
    

    bool IsAbilityDuplicate(Ability ability) => Ability.Where(a => a != null)
        .Any(a => a.Name == ability.Name);


    public AbilityInsertState validateAbilityInsert(Ability ability)
    {
        if (IsAbilityDuplicate(ability) || MAX_AMOUNT_ABILITIES <= IdAbility) 
            return  AbilityInsertState.Unviable;
        if (ability.Cost > Attributes.CurrentMp )  
            return AbilityInsertState.Incorrect;
        return AbilityInsertState.Correct;
    }
    public void AddAbility(Ability ability)
    {
        Ability[IdAbility] = ability;
        IdAbility++;
    }

    
    protected Unit()
    {
        Ability = new Ability[MAX_AMOUNT_ABILITIES];
    }

    public int Attack(Unit target, ElementType elementType)
    {
        var modifier = elementType == ElementType.Physics ? MODIFIER_PHYSICS : MODIFIER_GUN;
        var statAttack = elementType == ElementType.Physics ? Attributes.StrikeDmg : Attributes.SkillDmg;
        var damageDone = (int)(modifier * statAttack * ATTACK_CONST_JUST);
        
        target.TakeDamage(damageDone);
        
        return damageDone;
    }
    
    public void TakeDamage(int damage)
    {
        Attributes.CurrentHp -= damage;
        if (Attributes.CurrentHp < 0)
        {
            Attributes.CurrentHp = 0;
        }
    }
    
    public int GetTotalAbilities() => Ability.Where(x => x!= null).ToArray().Length;
}