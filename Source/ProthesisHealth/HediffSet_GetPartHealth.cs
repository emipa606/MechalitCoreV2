using HarmonyLib;
using UnityEngine;
using Verse;

namespace ProthesisHealth;

[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.GetPartHealth))]
public class HediffSet_GetPartHealth
{
    private static bool Prefix(ref HediffSet __instance, ref BodyPartRecord part, ref float __result)
    {
        if (part == null)
        {
            __result = 0f;
        }
        else
        {
            var num = part.def.GetMaxHealth(__instance.pawn);
            foreach (var hediff in __instance.hediffs)
            {
                if (hediff is Hediff_MissingPart && hediff.Part == part)
                {
                    __result = 0f;
                    return false;
                }

                if (hediff.Part != part)
                {
                    continue;
                }

                var hediffComp_PartHitPoints = hediff.TryGetComp<HediffComp_PartHitPoints>();
                if (hediffComp_PartHitPoints != null)
                {
                    num *= hediffComp_PartHitPoints.Props.multiplier;
                }

                if (hediff is Hediff_Injury hediff_Injury)
                {
                    num -= hediff_Injury.Severity;
                }
            }

            num = Mathf.Max(num, 0f);
            if (!part.def.destroyableByDamage)
            {
                num = Mathf.Max(num, 1f);
            }

            __result = Mathf.RoundToInt(num);
        }

        return false;
    }
}