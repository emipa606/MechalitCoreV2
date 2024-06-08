using Verse;

namespace ProthesisHealth;

public class HediffCompProperties_PartHitPoints : HediffCompProperties
{
    public float multiplier;

    public HediffCompProperties_PartHitPoints()
    {
        compClass = typeof(HediffComp_PartHitPoints);
    }
}