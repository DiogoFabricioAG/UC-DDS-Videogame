namespace Shin_Megami_Tensei;

public class MultiHitController
{
    public static int HandleMultiHit(int k, int lowerRange, int upperRange) => lowerRange + (k % (upperRange- lowerRange  + 1));
    
}