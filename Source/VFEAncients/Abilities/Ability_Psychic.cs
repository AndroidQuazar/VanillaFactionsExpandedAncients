using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VFEAncients
{
    public class Ability_Psychic : Ability
    {
        public override void ApplyHediffs(LocalTargetInfo targetInfo)
        {
            if (targetInfo.Pawn is null) return;
            var hediffExtension = def.GetModExtension<AbilityExtension_Hediff>();
            if (!(hediffExtension?.applyAuto ?? false)) return;
            var localHediff = HediffMaker.MakeHediff(hediffExtension.hediff, targetInfo.Pawn);
            if (hediffExtension.severity > float.Epsilon)
                localHediff.Severity = hediffExtension.severity;
            if (localHediff is HediffWithComps hwc)
                foreach (var hediffComp in hwc.comps)
                    switch (hediffComp)
                    {
                        case HediffComp_Ability hca:
                            hca.ability = this;
                            break;
                        case HediffComp_Disappears hcd:
                            hcd.ticksToDisappear = Mathf.RoundToInt(hcd.ticksToDisappear * targetInfo.Pawn.GetStatValue(StatDefOf.PsychicSensitivity));
                            break;
                    }

            targetInfo.Pawn.health.AddHediff(localHediff);
        }
    }
}