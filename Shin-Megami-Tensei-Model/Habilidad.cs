namespace Shin_Megami_Tensei_Model;

public class Habilidad
{
    public string name { get; set; }
    public string type { get; set; }
    public int cost { get; set; }
    public int power { get; set; }
    public string target { get; set; }
    public string hits { get; set; }
    public string effect { get; set; }

    public Habilidad(string name)
    {
        this.name = name;
    }
    
}