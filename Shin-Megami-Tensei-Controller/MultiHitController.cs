using Shin_Megami_Tensei_Model;

namespace Shin_Megami_Tensei;

public abstract class MultiHitController
{
    private static int HandleMultiHit(int k, int lowerRange, int upperRange) => lowerRange + (k % (upperRange- lowerRange  + 1));
    public static int GetHits(string hits, int numAbilitiesCast)
    {
        if (!hits.Contains('-')) return 1;
        var split = hits.Split('-');
        var lower = int.Parse(split[0]);
        var upper = int.Parse(split[1]);
        return HandleMultiHit(numAbilitiesCast, lower, upper);
    }

    public static List<int> GetHitsForAttackers(int numberHits, Team team, int numAbilitiesCast)
    {
        int A = team.GetUnitsStillAlive().Length;
        int i = numAbilitiesCast % A;
        bool D = i % 2 == 0;
        List<Unit> unitsInOrden = team.GetUnitsStillAlive().ToList();
        List<int> numhits = unitsInOrden.Select(x => 0).ToList();

        int addingNum = D ? 1 : -1;
        for (int pointer = 0; pointer < numberHits; pointer++)
        {
            numhits[i]++;
            i += addingNum;
            if (i >= A)
            {
                i = 0;
            }
            else if (i < 0)
            {
                i = A - 1;
            }
        }

        return numhits;
    }
}