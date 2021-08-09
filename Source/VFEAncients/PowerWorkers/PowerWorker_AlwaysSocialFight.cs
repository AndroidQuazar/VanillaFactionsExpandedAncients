using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_AlwaysSocialFight : PowerWorker
    {
        public PowerWorker_AlwaysSocialFight(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.SocialFightChance)),
                postfix: new HarmonyMethod(GetType(), nameof(ForceSocialFight)));
        }

        public static void ForceSocialFight(Pawn ___pawn, ref float __result)
        {
            if (__result > 0f && HasPower<PowerWorker_AlwaysSocialFight>(___pawn)) __result = 1.1f;
        }
    }
}