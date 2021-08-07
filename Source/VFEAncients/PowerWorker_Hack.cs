using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Hack : PowerWorker
    {
        public PowerWorker_Hack(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(CompHackable), nameof(CompHackable.Hack)), new HarmonyMethod(GetType(), nameof(InstantHack)));
        }

        public static void InstantHack(ref float amount, Pawn hacker, CompHackable __instance)
        {
            if (HasPower<PowerWorker_Hack>(hacker)) amount = __instance.defence + 1f;
        }
    }
}