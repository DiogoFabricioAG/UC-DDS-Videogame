namespace Shin_Megami_Tensei_Model.Enums;
using System.ComponentModel; 

public enum ActionType
{
    [Description("Atacar")]
    Attack,
    [Description("Disparar")]
    Shoot,
    [Description("Usar Habilidad")]
    Spell,
    [Description("Invocar")]
    Invoke,
    [Description("Pasar Turno")]
    Pass,
    [Description("Rendirse")]
    Surrender
}