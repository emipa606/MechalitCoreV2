using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ProthesisHealth;

[HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculatePartEfficiency))]
public class PawnCapacityUtility_CalculatePartEfficiency
{
    private static bool Prefix(ref HediffSet diffSet, BodyPartRecord part, ref bool ignoreAddedParts,
        ref List<PawnCapacityUtility.CapacityImpactor> impactors, ref float __result)
    {
        for (var parent = part.parent; parent != null; parent = parent.parent)
        {
            if (!diffSet.HasDirectlyAddedPartFor(parent))
            {
                continue;
            }

            var firstHediffMatchingPart = diffSet.GetFirstHediffMatchingPart<Hediff_AddedPart>(parent);
            impactors?.Add(new PawnCapacityUtility.CapacityImpactorHediff
            {
                hediff = firstHediffMatchingPart
            });
            __result = firstHediffMatchingPart.def.addedPartProps.partEfficiency;
            return false;
        }

        if (part.parent != null && diffSet.PartIsMissing(part.parent))
        {
            __result = 0f;
            return false;
        }

        var num = 1f;
        if (!ignoreAddedParts)
        {
            foreach (var hediff in diffSet.hediffs)
            {
                if (hediff is not Hediff_AddedPart hediff_AddedPart || hediff_AddedPart.Part != part)
                {
                    continue;
                }

                num *= hediff_AddedPart.def.addedPartProps.partEfficiency;
                if (hediff_AddedPart.def.addedPartProps.partEfficiency != 1f)
                {
                    impactors?.Add(new PawnCapacityUtility.CapacityImpactorHediff
                    {
                        hediff = hediff_AddedPart
                    });
                }
            }
        }

        var b = -1f;
        var num2 = 0f;
        var missingHp = false;
        foreach (var hediff in diffSet.hediffs)
        {
            if (hediff.Part != part || hediff.CurStage == null)
            {
                continue;
            }

            var curStage = hediff.CurStage;
            num2 += curStage.partEfficiencyOffset;
            missingHp |= curStage.partIgnoreMissingHP;
            if (curStage.partEfficiencyOffset != 0f && curStage.becomeVisible)
            {
                impactors?.Add(new PawnCapacityUtility.CapacityImpactorHediff
                {
                    hediff = hediff
                });
            }
        }

        if (!missingHp)
        {
            var maxHealth = part.def.GetMaxHealth(diffSet.pawn);
            maxHealth = Mathf.RoundToInt(maxHealth);
            foreach (var item in diffSet.pawn.health.hediffSet.hediffs.Where(x => x.Part == part))
            {
                var hediffComp_PartHitPoints = item.TryGetComp<HediffComp_PartHitPoints>();
                if (hediffComp_PartHitPoints != null)
                {
                    maxHealth *= hediffComp_PartHitPoints.Props.multiplier;
                }
            }

            var num3 = diffSet.GetPartHealth(part) / maxHealth;
            if (num3 != 1f)
            {
                if (DamageWorker_AddInjury.ShouldReduceDamageToPreservePart(part))
                {
                    num3 = Mathf.InverseLerp(0.1f, 1f, num3);
                }

                impactors?.Add(new PawnCapacityUtility.CapacityImpactorBodyPartHealth
                {
                    bodyPart = part
                });
                num *= num3;
            }
        }

        num += num2;
        if (num > 0.0001f)
        {
            num = Mathf.Max(num, b);
        }

        __result = Mathf.Max(num, 0f);
        return false;
    }
}