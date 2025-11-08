namespace Shin_Megami_Tensei_Model.dtos;

public class HpAndMpResponseSingle(int hpDrain,int mpDrain)
{
    public int HpDrain { get;  } = hpDrain;
    public int MpDrain { get;  } = mpDrain;

}