using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Bones : PowerWorker
    {
        public PowerWorker_Bones(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(DamageWorker_AddInjury), "ApplyDamageToPart"), transpiler: new HarmonyMethod(GetType(), nameof(ApplyDamageToPart_Transpile)));
        }

        public static IEnumerable<CodeInstruction> ApplyDamageToPart_Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Method(typeof(DamageInfo), nameof(DamageInfo.SetHitPart));
            var idx1 = list.FindIndex(ins => ins.Calls(info1));
            var label1 = generator.DefineLabel();
            list[idx1 + 1].labels.Add(label1);
            list.InsertRange(idx1 + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerWorker), nameof(HasPower), generics: new[] {typeof(PowerWorker_Bones)})),
                new CodeInstruction(OpCodes.Brfalse, label1),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BodyPartRecord), nameof(BodyPartRecord.def))),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.health))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.hediffSet))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(HediffSet), nameof(HediffSet.hediffs))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BodyPartDef), nameof(BodyPartDef.IsSolid))),
                new CodeInstruction(OpCodes.Brfalse, label1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.health))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.hediffSet))),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HediffSet), nameof(HediffSet.PartOrAnyAncestorHasDirectlyAddedParts))),
                new CodeInstruction(OpCodes.Brtrue, label1),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(DamageWorker.DamageResult), nameof(DamageWorker.DamageResult.AddPart))),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(DamageWorker.DamageResult), nameof(DamageWorker.DamageResult.deflected))),
                new CodeInstruction(OpCodes.Ret)
            });
            return list;
        }
    }
}