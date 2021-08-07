using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEAncients
{
    public class PowerWorker_BreakOnReject : PowerWorker
    {
        private static readonly Dictionary<Pawn, Pawn> forcedTargets = new Dictionary<Pawn, Pawn>();

        public PowerWorker_BreakOnReject(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new[] {typeof(ThoughtDef), typeof(Pawn), typeof(Precept)}),
                postfix: new HarmonyMethod(GetType(), nameof(PostMemoryAdd)));
            harm.Patch(AccessTools.Method(typeof(MentalState_MurderousRage), "TryFindNewTarget"), new HarmonyMethod(GetType(), nameof(ForceTarget)));
        }

        public static void PostMemoryAdd(MemoryThoughtHandler __instance, Pawn otherPawn, ThoughtDef def)
        {
            if (def == ThoughtDefOf.RebuffedMyRomanceAttempt && HasPower<PowerWorker_BreakOnReject>(__instance.pawn))
            {
                var data = GetData<WorkerData_Break>(__instance.pawn);
                if (data != null && Rand.Chance(data.BreakChance))
                {
                    forcedTargets[__instance.pawn] = otherPawn;
                    data.Break.Worker.TryStart(__instance.pawn, ThoughtDefOf.RebuffedMyRomanceAttempt.stages[0].label.CapitalizeFirst(), false);
                }
            }
        }

        public static bool ForceTarget(MentalState_MurderousRage __instance, ref bool __result)
        {
            if (HasPower<PowerWorker_BreakOnReject>(__instance.pawn) && forcedTargets.TryGetValue(__instance.pawn, out var otherPawn))
            {
                __instance.target = otherPawn;
                __result = __instance.IsTargetStillValidAndReachable();
                return false;
            }

            return true;
        }
    }

    public class WorkerData_Break : WorkerData
    {
        public MentalBreakDef Break;
        public float BreakChance;
    }
}