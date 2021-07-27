using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_MeleeSkill : PowerWorker
    {
        public PowerWorker_MeleeSkill(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "GetNonMissChance"), new HarmonyMethod(GetType(), nameof(ForceHit)));
            harm.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "GetDodgeChance"), new HarmonyMethod(GetType(), nameof(ForceDodge)));
        }

        public static bool ForceHit(Verb_MeleeAttack __instance, ref float __result)
        {
            if (IsSkilled(__instance.Caster))
            {
                __result = 1f;
                return false;
            }

            return true;
        }

        public static bool ForceDodge(LocalTargetInfo target, ref float __result)
        {
            if (target.HasThing && IsSkilled(target.Thing))
            {
                __result = 1f;
                return false;
            }

            return true;
        }

        public static bool IsSkilled(Thing caster)
        {
            return caster is Pawn pawn && (pawn.GetPowerTracker()?.AllPowers.Any(power => power?.Worker is PowerWorker_MeleeSkill) ?? false);
        }
    }
}