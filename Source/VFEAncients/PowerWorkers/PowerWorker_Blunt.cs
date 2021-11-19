using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Blunt : PowerWorker
    {
        public PowerWorker_Blunt(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.PreApplyDamage)), new HarmonyMethod(GetType(), nameof(ChangeType)));
        }

        public static void ChangeType(Pawn __instance, ref DamageInfo dinfo)
        {
            if (HasPower<PowerWorker_Blunt>(__instance))
            {
                dinfo.Def = DamageDefOf.Blunt;
                dinfo.SetBodyRegion(depth: BodyPartDepth.Outside);
                dinfo.SetAllowDamagePropagation(false);
            }
        }
    }
}