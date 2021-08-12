using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine.Assertions;
using Verse;

namespace VFEAncients.HarmonyPatches
{
    public static class Helpers
    {
        public static bool HasPower(this Pawn pawn, PowerDef power)
        {
            return pawn.GetPowerTracker()?.HasPower(power) ?? false;
        }
    }

    public static class PowerPatches
    {
        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.ExposeData)), postfix: new HarmonyMethod(typeof(Pawn_PowerTracker), nameof(Pawn_PowerTracker.Save)));
            harm.Patch(AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized)),
                transpiler: new HarmonyMethod(typeof(PowerPatches), nameof(StatGetValueTranspile)));
            harm.Patch(AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetExplanationUnfinalized)),
                transpiler: new HarmonyMethod(typeof(PowerPatches), nameof(StatExplanationTranspile)));
            harm.Patch(AccessTools.Method(typeof(Pawn_InteractionsTracker), "TryInteractRandomly"), transpiler: new HarmonyMethod(typeof(PowerPatches), nameof(ForceInteraction)));
            harm.Patch(AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new[] {typeof(Tool), typeof(Pawn), typeof(Thing)}),
                postfix: new HarmonyMethod(typeof(PowerPatches), nameof(ApplyStat)));
            harm.Patch(AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new[] {typeof(Tool), typeof(Pawn), typeof(ThingDef), typeof(ThingDef)}),
                postfix: new HarmonyMethod(typeof(PowerPatches), nameof(ApplyStat)));
        }

        public static void ApplyStat(ref float __result, Pawn attacker, VerbProperties __instance)
        {
            if (attacker != null && __instance.IsMeleeAttack) __result *= attacker.GetStatValue(VFEA_DefOf.VFEAncients_MeleeCooldownFactor);
        }

        public static IEnumerable<CodeInstruction> ForceInteraction(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.opcode == OpCodes.Call && ins.operand is MethodInfo mi && mi.Name == "TryRandomElementByWeight") + 2;
            Assert.AreEqual(list[idx1].opcode, OpCodes.Ldarg_0);
            var label1 = generator.DefineLabel();
            var label2 = generator.DefineLabel();
            list[idx1].labels.Add(label1);
            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn")),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(VFEA_DefOf), nameof(VFEA_DefOf.Lustful))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.HasPower))),
                new CodeInstruction(OpCodes.Brfalse, label2),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(VFEA_DefOf), nameof(VFEA_DefOf.VFEA_RomanceAttempt_Lustful))),
                new CodeInstruction(OpCodes.Stloc, 4),
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(label2),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn")),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(VFEA_DefOf), nameof(VFEA_DefOf.Celebrity))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.HasPower))),
                new CodeInstruction(OpCodes.Brfalse, label1),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(VFEA_DefOf), nameof(VFEA_DefOf.KindWords))),
                new CodeInstruction(OpCodes.Stloc, 4)
            });
            return list;
        }

        public static IEnumerable<CodeInstruction> StatGetValueTranspile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Field(typeof(Pawn), nameof(Pawn.ageTracker));
            var idx1 = list.FindIndex(ins => ins.LoadsField(info1));
            var idx2 = list.FindIndex(idx1, ins => ins.opcode == OpCodes.Stloc_0);
            list.InsertRange(idx2 + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloca_S, 0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerPatches), nameof(PowerModifyStat)))
            });
            return list;
        }

        public static void PowerModifyStat(Pawn pawn, ref float val, StatDef stat)
        {
            if (pawn.GetPowerTracker() is Pawn_PowerTracker tracker)
                foreach (var power in tracker.AllPowers)
                {
                    if (power.statFactors != null) val *= power.statFactors.GetStatFactorFromList(stat);
                    if (power.statOffsets != null) val += power.statOffsets.GetStatOffsetFromList(stat);
                }
        }

        public static IEnumerable<CodeInstruction> StatExplanationTranspile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Field(typeof(Pawn), nameof(Pawn.ageTracker));
            var idx1 = list.FindIndex(ins => ins.LoadsField(info1));
            var label1 = (Label) list[list.FindIndex(idx1, ins => ins.opcode == OpCodes.Beq_S)].operand;
            var idx2 = list.FindIndex(idx1, ins => ins.opcode == OpCodes.Pop);
            list[idx2 + 1].labels.Remove(label1);
            list.InsertRange(idx2 + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_2).WithLabels(label1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerPatches), nameof(PowerModifyExplanation)))
            });
            return list;
        }

        public static void PowerModifyExplanation(Pawn pawn, StringBuilder builder, StatDef stat, StatWorker worker)
        {
            if (pawn.GetPowerTracker() is Pawn_PowerTracker tracker)
                foreach (var power in tracker.AllPowers)
                {
                    if (power.statFactors != null)
                    {
                        var factor = power.statFactors.GetStatFactorFromList(stat);
                        if (Math.Abs(factor - 1f) > 0.0001f)
                            builder.AppendLine(power.LabelCap + ": " + worker.ValueToString(factor, false, ToStringNumberSense.Factor));
                    }

                    if (power.statOffsets != null)
                    {
                        var offset = power.statOffsets.GetStatOffsetFromList(stat);
                        if (offset != 0f)
                            builder.AppendLine(power.LabelCap + ": " + worker.ValueToString(offset, false, ToStringNumberSense.Offset));
                    }
                }
        }
    }
}