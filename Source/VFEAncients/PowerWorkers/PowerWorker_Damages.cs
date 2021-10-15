using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Damages : PowerWorker
    {
        public PowerWorker_Damages(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.TakeDamage)), new HarmonyMethod(GetType(), nameof(ChangeDamage)));
        }

        public static void ChangeDamage(ref DamageInfo dinfo, Thing __instance)
        {
            if (HasPower<PowerWorker_Damages>(__instance))
            {
                var def = dinfo.Def;
                var data = GetData<WorkerData_Damages>(__instance);
                if (data.Resist.Contains(def)) dinfo.SetAmount(0f);
                if (data.Multipliers.FirstOrDefault(m => m.damageDef == def) is {multiplier: var mult}) dinfo.SetAmount(dinfo.Amount * mult);
            }
        }
    }

    public class WorkerData_Damages : WorkerData
    {
        public List<DamageMultiplier> Multipliers;
        public List<DamageDef> Resist;
    }
}