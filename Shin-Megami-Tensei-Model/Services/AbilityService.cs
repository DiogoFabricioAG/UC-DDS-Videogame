namespace Shin_Megami_Tensei_Model.Services;

public class AbilityService
{
    public void UseAbility(Unit user, Ability ability)
    {
        if (user.Attributes.CurrentMp < ability.Cost)
        {
            throw new InvalidOperationException("No hay suficiente MP para usar esta habilidad.");
        }
        
        // ability.ApplyEffects(user); # Aun no lo tenemos :D
    }
}