using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Celebrity : PowerWorker
    {
        public PowerWorker_Celebrity(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(ThoughtHandler), nameof(ThoughtHandler.OpinionOffsetOfGroup)), postfix: new HarmonyMethod(GetType(), nameof(Double)));
        }

        public static void Double(Pawn otherPawn, ref int __result)
        {
            if (HasPower<PowerWorker_Celebrity>(otherPawn)) __result *= 2;
        }
    }

    [DefOf]
    public static class CelebrityDefOf
    {
        public static InteractionDef KindWords;
        public static PowerDef Celebrity;
    }
}