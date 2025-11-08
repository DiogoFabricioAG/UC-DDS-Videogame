namespace Shin_Megami_Tensei_Model.dtos;

public class HpAndMpResponseList (List<int> HpDrainTeam, List<int> MpDrainTeam)
{
    public List<int> HpDrainTeam { get; set; } = HpDrainTeam;
    public List<int> MpDrainTeam { get; set; } = MpDrainTeam;

}