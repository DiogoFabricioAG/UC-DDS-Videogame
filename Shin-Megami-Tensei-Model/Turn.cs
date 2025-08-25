namespace Shin_Megami_Tensei_Model;

public class Turn
{
    private TurnType _type;

    public TurnType Type
    {
        get => _type; 
        set => _type = value; 
    } 

    public Turn(TurnType type)
    {
        _type = type;
    }}