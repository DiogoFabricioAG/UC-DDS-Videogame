using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_Model.UnitsEnums;

namespace Shin_Megami_Tensei_Model;

public abstract class Unit
{
    private const int MAX_AMOUNT_ABILITIES = 8;

    private int AbilityIndex
    {
        get;
        set;
    }
    public string Name { get; set; }
    public Attributes Attributes { get; set ; }
    public Affinity Affinity  { get ; set ; }
    
    public Ability[] Abilities  { get ; set ; } 

    public string GetStatus() => $"{Name} HP:{Attributes.CurrentHp}/{Attributes.MaxHp} MP:{Attributes.CurrentMp}/{Attributes.MaxMp}";
    
    
    public virtual List<ActionType> GetAvailableActions()
    {
        // El set más amplio para que solo las subclases lo eliminen.
        return
        [
            ActionType.Attack, 
            ActionType.Shoot,    
            ActionType.Spell,  
            ActionType.Invoke, 
            ActionType.Pass,   
            ActionType.Surrender
        ];
    }
    
    public bool Defeated { get; set; } = false;

    public void ChangeStatus()
    {
        Defeated = !Defeated;
    }
    
    bool IsAbilityDuplicate(Ability ability) => Abilities.Where(a => a != null)
        .Any(a => a.Name == ability.Name);


    public AbilityInsertState ValidateAbilityInsert(Ability ability)
    {
        if (IsAbilityDuplicate(ability) || MAX_AMOUNT_ABILITIES <= AbilityIndex) 
            return  AbilityInsertState.Unviable;
        if (ability.Cost > Attributes.CurrentMp )  
            return AbilityInsertState.Incorrect;
        return AbilityInsertState.Correct;
    }
    public void AddAbility(Ability ability)
    {
        Abilities[AbilityIndex] = ability;
        AbilityIndex++;
    }
    
    public void ShowAbility()
    {
        foreach (var ability in Abilities.Where(a => a != null))
        {
            Console.WriteLine(ability.Name);
        }
    }
    
    protected Unit()
    {
        Abilities = new Ability[MAX_AMOUNT_ABILITIES];
    }

    public void HandleDamage(int damage)
    {
        
        Attributes.CurrentHp -= damage;
        if (Attributes.CurrentHp < 0)
        {
            Attributes.CurrentHp = 0;
        }

        if (Attributes.CurrentHp > Attributes.MaxHp)
        {
            Attributes.CurrentHp = Attributes.MaxHp;
        }
    }

    public void ValidUseAbility(Ability ability)
    {
        if (Attributes.CurrentMp < ability.Cost)
        {
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
        }
    }

    public void BurnManaPoints(Ability ability)
    {
        Attributes.CurrentMp -= ability.Cost;
    }
    public Ability[] GetTotalAbilities() => Abilities.Where(x => x!= null && x.Cost <= Attributes.CurrentMp).ToArray();
}