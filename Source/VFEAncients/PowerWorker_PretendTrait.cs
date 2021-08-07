using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_PretendTrait : PowerWorker
    {
        public PowerWorker_PretendTrait(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new[] {typeof(TraitDef)}), postfix: new HarmonyMethod(GetType(), nameof(HasTrait_Postfix)));
            harm.Patch(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new[] {typeof(TraitDef), typeof(int)}),
                postfix: new HarmonyMethod(GetType(), nameof(HasTrait_Postfix)));
        }

        public static void HasTrait_Postfix(TraitDef tDef, Pawn ___pawn, ref bool __result)
        {
            if (!__result && HasPower<PowerWorker_PretendTrait>(___pawn)) __result = GetData<WorkerData_Trait>(___pawn)?.Trait == tDef;
        }
    }

    public class WorkerData_Trait : WorkerData
    {
        public TraitDef Trait;
    }
}