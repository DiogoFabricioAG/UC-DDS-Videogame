namespace Shin_Megami_Tensei;

public abstract class MultiHitController
{
    public static int HandleMultiHit(int k, int lowerRange, int upperRange) => lowerRange + (k % (upperRange- lowerRange  + 1));
    
}