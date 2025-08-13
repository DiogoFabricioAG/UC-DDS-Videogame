namespace Shin_Megami_Tensei_Model;

public interface Personaje
{
    string nombre {get; set; }
    Atributos atributos {get; set; }
    Afinidad afinidad {get; set; }
}