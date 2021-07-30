using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_PrefersDarkness : PowerWorker
    {
        public PowerWorker_PrefersDarkness(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(StatPart_Glow), "ActiveFor"), postfix: new HarmonyMethod(GetType(), nameof(Glow_ActiveFor_Postfix)));
        }

        public static void Glow_ActiveFor_Postfix(Thing t, ref bool __result)
        {
            if (__result && HasPower<PowerWorker_PrefersDarkness>(t)) __result = false;
        }
    }
}