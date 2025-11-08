using Shin_Megami_Tensei_Model;
using Shin_Megami_Tensei_Model.dtos;
using Shin_Megami_Tensei_Model.Enums;
using Shin_Megami_Tensei_Model.Extensions;
using Shin_Megami_Tensei_View.ConsoleLib;

namespace Shin_Megami_Tensei_View;

public class View 
{
    private const string SEPARATOR = "----------------------------------------";
    private readonly char[] LABELMAXUNITSONTABLE = { 'A', 'B', 'C', 'D' };
    private readonly DisplayFormatter formatter = new DisplayFormatter();
    private readonly AbstractView _view;

    public static View BuildConsoleView()
        => new View(new ConsoleView());

    public static View BuildTestingView(string pathTestScript)
        => new View(new TestingView(pathTestScript));

    public static View BuildManualTestingView(string pathTestScript)
        => new View(new ManualTestingView(pathTestScript));
    
    private View(AbstractView newView)
    {
        _view = newView;
    }
    
    public string ReadLine()
    {
        return _view.ReadLine();
    }

    public void WriteLine(string message)
    {
        _view.WriteLine(message);
    }
    
    public void DisplayUnitActions(string unitName, List<ActionType> actions)
    {
        WriteLine($"Seleccione una acción para {unitName}");
        for (var i = 0; i < actions.Count; i++)
        {
            WriteLine($"{i + 1}: {actions[i].GetDescription()}");
        }
    }
    public void DisplayShowSelectableAbilities(Unit unit)
    {
        int counter = 1;
        foreach (var ability in unit.GetTotalAbilities())
        {
            WriteLine($"{counter}-{ability.GetPresentation()}");
            counter++;
        }
        WriteLine($"{counter}-Cancelar") ;
    }
    
    public void DisplayCurrentTurnOrder(Team team)
    {
        WriteLine("Orden:");
        Console.WriteLine(team.TeamTurnOrder);
        foreach (var orderTurns in formatter.FormatTurnOrder(team))
        {
            WriteLine(orderTurns);
        }
        WriteLine(SEPARATOR);
    }
    

    
    public void DisplayShowSelectablesUnit(Team otherTeam,Team currentTeam, TargetType targetType, bool showThemAll = false)
    {
        WriteLine($"Seleccione un objetivo para {currentTeam.GetUnitInTurn().Name}");
       
        var (selectableUnits, cancelOption) = formatter.FormatSelectableUnits(otherTeam, currentTeam, targetType, showThemAll);

        foreach (var unit in selectableUnits)
        {
            WriteLine(unit);
        }
        WriteLine(cancelOption);
    }
    
    public void DisplayCurrentTurnsbyType(Team team)
    {
        WriteLine($"Full Turns: {team.GetCurrentFullTurns()}");
        WriteLine($"Blinking Turns: {team.GetCurrentBlinkingTurns()}");
        WriteLine(SEPARATOR);
    }
    
    public void DisplayPlayerTurnExclamation(Team team)
    {
        WriteLine($"Ronda de {team.GetName()}\n{SEPARATOR}");
    }
    
    public void DisplayTeamsUnitsCurrentStatus(Game game)
    {
        var (team1, team2) = game.GetPlayers();
        foreach (var statusLogs in formatter.FormatTeamsStatusTable(team1, team2))
        {
            WriteLine(statusLogs);
        }
        WriteLine(SEPARATOR);
    }
    
    public void DisplayAbilityLogs(int attackDamage, Unit attacker, Unit attacked, AffinityType type, AbilityType abilityType, int numberHits, bool reviveUnit = false )
    {
        var affinityText = type == AffinityType.Weak ? "débil contra" : type == AffinityType.Resist ? "resistente" : string.Empty;
        var attackType = abilityType switch
        {
            AbilityType.Phys => "ataca",
            AbilityType.Gun => "dispara",
            AbilityType.Fire => "lanza fuego",
            AbilityType.Ice => "lanza hielo",
            AbilityType.Elec => "lanza electricidad",
            AbilityType.Force => "lanza viento",
            AbilityType.Light => "ataca con luz",
            AbilityType.Almighty => "lanza un ataque todo poderoso",

            AbilityType.Dark => "ataca con oscuridad",
            AbilityType.Heal => reviveUnit ? "revive" : "cura",
            _ => ""
        };

        var healOrDamage = abilityType != AbilityType.Heal ? "daño" : "HP";
        
        for (var i = 0; i < numberHits; i++)
        {
            WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");
            switch (type)
            {
                case AffinityType.Weak:
                case AffinityType.Resist:
                    WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");
                    WriteLine($"{attacked.Name} recibe {attackDamage} de daño");
                    break;
                case AffinityType.Null:
                    WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                    break;
                case AffinityType.Repel:
                    WriteLine($"{attacked.Name} devuelve {attackDamage} daño a {attacker.Name}");
                    break;
                case AffinityType.Drain:
                    WriteLine($"{attacked.Name} absorbe {Math.Abs(attackDamage)} daño");
                    break;
                case AffinityType.Neutral:
                default:
                    WriteLine($"{attacked.Name} recibe {attackDamage} de {healOrDamage}");
                    break;
            }
        }

        WriteLine(type == AffinityType.Repel
            ? $"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}"
            : $"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
        WriteLine(SEPARATOR);
    }
    
    public void DisplayDrainAbilityLogs(int attackDamage, Unit attacker, Unit attacked, bool isHpAbility )
    {
        string HPOrMp = isHpAbility ? "HP" : "MP";
        WriteLine($"{attacker.Name} lanza un ataque todo poderoso a {attacked.Name}");
        WriteLine($"El ataque drena {attackDamage} {HPOrMp} de {attacked.Name}");
        
        int attackerCurrentStat = isHpAbility ? attacker.Attributes.CurrentHp : attacker.Attributes.CurrentMp;
        int attackerMaxStat = isHpAbility ? attacker.Attributes.MaxHp : attacker.Attributes.MaxMp;
        int attackedCurrentStat = isHpAbility ? attacked.Attributes.CurrentHp : attacked.Attributes.CurrentMp;
        int attackedMaxStat = isHpAbility ? attacked.Attributes.MaxHp : attacked.Attributes.MaxMp;
        
        WriteLine($"{attacked.Name} termina con {HPOrMp}:{attackedCurrentStat}/{attackedMaxStat}");
        WriteLine($"{attacker.Name} termina con {HPOrMp}:{attackerCurrentStat}/{attackerMaxStat}");

        WriteLine(SEPARATOR);
    }

    public void DisplayDrainAllAbilityLogs(List<int> attackDamages, Unit attacker, Team team, bool isHpAbility)
    {
        int pointer = 0;
        foreach (var attacked in team.GetUnitsStillAlive())
        {
            string HPOrMp = isHpAbility ? "HP" : "MP";
            WriteLine($"{attacker.Name} lanza un ataque todo poderoso a {attacked.Name}");
            WriteLine($"El ataque drena {attackDamages[pointer]} {HPOrMp} de {attacked.Name}");
        
            int attackerCurrentStat = isHpAbility ? attacker.Attributes.CurrentHp : attacker.Attributes.CurrentMp;
            int attackerMaxStat = isHpAbility ? attacker.Attributes.MaxHp : attacker.Attributes.MaxMp;
            int attackedCurrentStat = isHpAbility ? attacked.Attributes.CurrentHp : attacked.Attributes.CurrentMp;
            int attackedMaxStat = isHpAbility ? attacked.Attributes.MaxHp : attacked.Attributes.MaxMp;
        
            WriteLine($"{attacked.Name} termina con {HPOrMp}:{attackedCurrentStat}/{attackedMaxStat}");
            WriteLine($"{attacker.Name} termina con {HPOrMp}:{attackerCurrentStat}/{attackerMaxStat}");
            pointer++;
        }
        WriteLine(SEPARATOR);
    }
    public void DisplayDrainStatsAbilityLogs(int hpDrain, int mpDrain, Unit attacker, Unit attacked )
    {
        WriteLine($"{attacker.Name} lanza un ataque todo poderoso a {attacked.Name}");
        WriteLine($"El ataque drena {hpDrain} HP de {attacked.Name}");
        
        WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
        WriteLine($"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}");
        
        WriteLine($"El ataque drena {mpDrain} MP de {attacked.Name}");
        WriteLine($"{attacked.Name} termina con MP:{attacked.Attributes.CurrentMp}/{attacked.Attributes.MaxMp}");
        WriteLine($"{attacker.Name} termina con MP:{attacker.Attributes.CurrentMp}/{attacker.Attributes.MaxMp}");

        WriteLine(SEPARATOR);
    }

    public void DisplayDrainStatsAllAbilityLogs(List<int> hpDrains, List<int> mpDrains, Unit attacker, Team team)
    {
        int pointer = 0;
        foreach (var attacked in team.GetUnitsStillAlive())
        {
            WriteLine($"{attacker.Name} lanza un ataque todo poderoso a {attacked.Name}");
            WriteLine($"El ataque drena {hpDrains[pointer]} HP de {attacked.Name}");
            WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
            if (pointer == hpDrains.Count - 1)
                WriteLine($"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}");

            WriteLine($"El ataque drena {mpDrains[pointer]} MP de {attacked.Name}");
            WriteLine($"{attacked.Name} termina con MP:{attacked.Attributes.CurrentMp}/{attacked.Attributes.MaxMp}");
            if (pointer == hpDrains.Count - 1)
                WriteLine($"{attacker.Name} termina con MP:{attacker.Attributes.CurrentMp}/{attacker.Attributes.MaxMp}");

            
            pointer++;
        }

        WriteLine(SEPARATOR);

    }
    


    public void DisplayAbilityAllLogs(List<int> quantityDone, Unit attacker, Team team, AffinityType affinityType
        , bool sacrific = false)
    {
        int pointer = 0;
        foreach (var attacked in sacrific ?  team.AllUnitsExceptInTurn(attacker) : team.HelperSelectableUnitsForHealAbilities(attacker)  )
        {
            var attackType = team.DestroyedUnits.Contains(attacked) ? "revive" : "cura";

            

            WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");
            switch (affinityType)
            {
                case AffinityType.Weak:
                case AffinityType.Resist:
                    WriteLine($"{attacked.Name} recibe {quantityDone[pointer]} de daño");
                    break;
                case AffinityType.Null:
                    WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                    break;
                case AffinityType.Repel:
                    WriteLine($"{attacked.Name} devuelve {quantityDone[pointer]} daño a {attacker.Name}");
                    break;
                case AffinityType.Drain:
                    WriteLine($"{attacked.Name} absorbe {Math.Abs(quantityDone[pointer])} daño");
                    break;
                case AffinityType.Neutral:
                default:
                    WriteLine($"{attacked.Name} recibe {quantityDone[pointer]} de HP");
                    break;
            }

            WriteLine(affinityType == AffinityType.Repel
                ? $"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}"
                : $"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
            pointer++;
        }
        if (sacrific)
            WriteLine($"{attacker.Name} termina con HP:0/{attacker.Attributes.MaxHp}");
        WriteLine(SEPARATOR);
    }
    public void DisplayAbilityAttackAll(List<int> quantityDone, Unit attacker, Team team, List<AffinityType> affinityTypes,
        AbilityType abilityType)
    {
        int pointer = 0;
       
        foreach (var attacked in 
                     team.GetUnitsStillAlive())
        {
            var affinityText = affinityTypes[pointer] == AffinityType.Weak ? "débil contra" : affinityTypes[pointer] == AffinityType.Resist ? "resistente" : string.Empty;

            var attackType = abilityType switch
            {
                AbilityType.Phys => "ataca",
                AbilityType.Gun => "dispara",
                AbilityType.Fire => "lanza fuego",
                AbilityType.Ice => "lanza hielo",
                AbilityType.Elec => "lanza electricidad",
                AbilityType.Force => "lanza viento",
                AbilityType.Light => "ataca con luz",
                AbilityType.Dark => "ataca con oscuridad",
                AbilityType.Almighty => "lanza un ataque todo poderoso",
                AbilityType.Heal => team.DestroyedUnits.Contains(attacked) ? "revive" : "cura",
                _ => ""
            };

            var healOrDamage = abilityType != AbilityType.Heal ? "daño" : "HP";


            WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");
            switch (affinityTypes[pointer])
            {
                case AffinityType.Weak:
                case AffinityType.Resist:
                    WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");

                    WriteLine($"{attacked.Name} recibe {quantityDone[pointer]} de daño");
                    break;
                case AffinityType.Null:
                    WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                    break;
                case AffinityType.Repel:
                    WriteLine($"{attacked.Name} devuelve {quantityDone[pointer]} daño a {attacker.Name}");
                    var lastRepel = true;
                    for (var i = pointer + 1; i < affinityTypes.Count; i++)
                    {
                        if (affinityTypes[i] == AffinityType.Repel)
                        {
                            lastRepel = false;
                            break;
                        }
                    }
                    if (lastRepel)
                        WriteLine($"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}");
                    break;
                case AffinityType.Drain:
                    WriteLine($"{attacked.Name} absorbe {Math.Abs(quantityDone[pointer])} daño");
                    break;
                case AffinityType.Neutral:
                default:
                    WriteLine($"{attacked.Name} recibe {quantityDone[pointer]} de {healOrDamage}");
                    break;
            }

            if (affinityTypes[pointer] != AffinityType.Repel)
            {
                WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
            }
            
            
            pointer++;
        }

       // if (affinityTypes.Contains(AffinityType.Repel))
       // {
       //     WriteLine($"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}");
//
       // }
        WriteLine(SEPARATOR);
    }
    
    public void DisplayAbilityMultiAttack(List<int> quantityDone, Unit attacker, Team team, List<int> numHits,List<AffinityType> affinityTypes,
        AbilityType abilityType)
    {
        int pointer = 0;
        
  
        foreach (var attacked in 
                     team.GetUnitsStillAlive())
        {
            var affinityText = affinityTypes[pointer] == AffinityType.Weak ? "débil contra" : affinityTypes[pointer] == AffinityType.Resist ? "resistente" : string.Empty;

            var attackType = abilityType switch
            {
                AbilityType.Phys => "ataca",
                AbilityType.Gun => "dispara",
                AbilityType.Fire => "lanza fuego",
                AbilityType.Ice => "lanza hielo",
                AbilityType.Elec => "lanza electricidad",
                AbilityType.Force => "lanza viento",
                AbilityType.Light => "ataca con luz",
                AbilityType.Dark => "ataca con oscuridad",
                AbilityType.Almighty => "lanza un ataque todo poderoso",
                AbilityType.Heal => team.DestroyedUnits.Contains(attacked) ? "revive" : "cura",
                _ => ""
            };

            var healOrDamage = abilityType != AbilityType.Heal ? "daño" : "HP";

            for (var i = 0; i < numHits[pointer]; i++)
            {
                WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");
                switch (affinityTypes[pointer])
                {
                    case AffinityType.Weak:
                    case AffinityType.Resist:
                        WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");

                        WriteLine($"{attacked.Name} recibe {quantityDone[pointer]} de daño");
                        break;
                    case AffinityType.Null:
                        WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                        break;
                    case AffinityType.Repel:
                        WriteLine($"{attacked.Name} devuelve {quantityDone[pointer]} daño a {attacker.Name}");
                        var lastRepel = true;
                        for (var ix = pointer + 1; ix < affinityTypes.Count; ix++)
                        {
                            if (affinityTypes[ix] == AffinityType.Repel)
                            {
                                lastRepel = false;
                                break;
                            }
                        }
                        if (i == numHits[pointer] - 1 && lastRepel)
                            WriteLine($"{attacker.Name} termina con HP:{attacker.Attributes.CurrentHp}/{attacker.Attributes.MaxHp}");

                        break;
                    case AffinityType.Drain:
                        WriteLine($"{attacked.Name} absorbe {Math.Abs(quantityDone[pointer])} daño");
                        break;
                    case AffinityType.Neutral:
                    default:
                        WriteLine($"{attacked.Name} recibe {quantityDone[pointer]} de {healOrDamage}");
                        break;
                }
            }
            

            if (affinityTypes[pointer] != AffinityType.Repel && numHits[pointer] != 0)
            {
                WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
            }
            
            
            pointer++;
        }
        WriteLine(SEPARATOR);
    }


    public void DisplayAbilityLightOrDark(Unit attacker, Unit attacked, LightOrDarkState state, AbilityType abilityType)
    {
       
        AffinityType affinity = attacked.Affinity.GetAffinity(abilityType);
        var affinityText = affinity == AffinityType.Weak ? "débil contra" :
            affinity == AffinityType.Resist ? "resistente" : string.Empty;
        var attackType = abilityType switch
        {
            AbilityType.Light => "ataca con luz",
            AbilityType.Dark => "ataca con oscuridad",
        };

        WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");

        switch (state)
        {
            case LightOrDarkState.Kill:
                if (affinityText != String.Empty)
                {
                    WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");
                }

                WriteLine($"{attacked.Name} ha sido eliminado");
                break;
            case LightOrDarkState.Miss:
                WriteLine($"{attacker.Name} ha fallado el ataque");
                break;
            case LightOrDarkState.Block:
                if (affinity == AffinityType.Resist)
                {
                    WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");
                }

                WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                break;
            case LightOrDarkState.Repel:
                WriteLine($"{attacked.Name} serias dudas");
                break;

        }

        WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");
        WriteLine(SEPARATOR);
        
    }

    public void DisplayAbilityLightOrDarkAll(List<LightOrDarkState> states, Unit attacker, Team team, AbilityType abilityType)
    {
        int pointer = 0;
    
        foreach (var attacked in team.GetSelectableUnitsForAllAttacks())
        {
            if (states[pointer] != LightOrDarkState.Empty)
            {
                AffinityType affinity = attacked.Affinity.GetAffinity(abilityType);
                var affinityText = affinity == AffinityType.Weak ? "débil contra" : affinity == AffinityType.Resist ? "resistente" : string.Empty;
                var attackType = abilityType switch
                {
                    AbilityType.Light => "ataca con luz",
                    AbilityType.Dark => "ataca con oscuridad",
                };

                WriteLine($"{attacker.Name} {attackType} a {attacked.Name}");
                
                switch (states[pointer])
                {
                    case LightOrDarkState.Kill:
                        if (affinityText != String.Empty)
                        {
                            WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");
                        }
                        WriteLine($"{attacked.Name} ha sido eliminado");
                        break;
                    case LightOrDarkState.Miss:
                        WriteLine($"{attacker.Name} ha fallado el ataque");
                        break;
                    case LightOrDarkState.Block:
                        if (affinity == AffinityType.Resist)
                        {
                            WriteLine($"{attacked.Name} es {affinityText} el ataque de {attacker.Name}");
                        }
                        WriteLine($"{attacked.Name} bloquea el ataque de {attacker.Name}");
                        break;
                    case LightOrDarkState.Repel:
                        WriteLine($"{attacked.Name} serias dudas");
                        break;

                }
                WriteLine($"{attacked.Name} termina con HP:{attacked.Attributes.CurrentHp}/{attacked.Attributes.MaxHp}");

            }
            
            pointer++;
        }

        WriteLine(SEPARATOR);
    }
    
    public void TurnUsedDisplayWithParameters(TurnContext ctx)
    {
        WriteLine($"Se han consumido {ctx.fullTurnLoss} Full Turn(s) y {ctx.blinkingTurnLoss} Blinking Turn(s)");
        WriteLine($"Se han obtenido {ctx.blinkingTurnWon} Blinking Turn(s)");
        WriteLine(SEPARATOR);
    }
    
    public void SurrenderTeamDisplay(Team team)
    {
        _view.WriteLine($"{team.GetName()} se rinde");
        _view.WriteLine(SEPARATOR);
    }
    
    public void ShowInvocableMonsters(Team team, bool showDefeatedUnitsToo = false)
    {
        WriteLine("Seleccione un monstruo para invocar");

        foreach (var invocableUnit in formatter.FormatInvocableUnits(team, showDefeatedUnitsToo))
        {
            WriteLine(invocableUnit);
        }
    }

    public void ShowReplaceableUnits(Team team)
    {
        WriteLine("Seleccione una posición para invocar");
        foreach (var replaceableUnit in formatter.FormatReplaceableUnits(team))
        {
            WriteLine(replaceableUnit);
        }
    }

    public void InvokeAnUnit(Unit unit, bool revived = false, Unit inTurn = null)
    {
        
        WriteLine($"{unit.Name} ha sido invocado");
        if (revived)
        {
            WriteLine($"{inTurn.Name} revive a {unit.Name}");
            WriteLine($"{unit.Name} recibe {unit.Attributes.MaxHp} de HP");
            WriteLine($"{unit.Name} termina con HP:{unit.Attributes.CurrentHp}/{unit.Attributes.MaxHp}");
        }
        WriteLine(SEPARATOR);
    }
    
    public string[] GetScript()
        => _view.GetScript();
}