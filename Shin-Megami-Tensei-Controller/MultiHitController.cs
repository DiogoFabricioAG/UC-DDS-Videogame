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
}