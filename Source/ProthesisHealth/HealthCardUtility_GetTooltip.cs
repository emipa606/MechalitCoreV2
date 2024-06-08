using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProthesisHealth;

[HarmonyPatch(typeof(HealthCardUtility), "GetTooltip")]
public class HealthCardUtility_GetTooltip
{
    private static bool Prefix(ref Pawn pawn, BodyPartRecord part, ref string __result)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{part.LabelCap}: ");
        var maxHealth = part.def.GetMaxHealth(pawn);
        maxHealth = Mathf.RoundToInt(maxHealth);
        foreach (var item in pawn.health.hediffSet.hediffs.Where(x => x.Part == part))
        {
            var hediffComp_PartHitPoints = item.TryGetComp<HediffComp_PartHitPoints>();
            if (hediffComp_PartHitPoints != null)
            {
                maxHealth *= hediffComp_PartHitPoints.Props.multiplier;
            }
        }

        maxHealth = Mathf.RoundToInt(maxHealth);
        stringBuilder.AppendLine($" {pawn.health.hediffSet.GetPartHealth(part)} / {maxHealth}");
        var num = PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, part);
        if (num != 1f)
        {
            stringBuilder.AppendLine("Efficiency".Translate() + ": " + num.ToStringPercent());
        }

        __result = stringBuilder.ToString();
        return false;
    }
}