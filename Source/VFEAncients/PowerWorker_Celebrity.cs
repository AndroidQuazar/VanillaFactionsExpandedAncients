using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine.Assertions;
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
            harm.Patch(AccessTools.Method(typeof(Pawn_InteractionsTracker), "TryInteractRandomly"), transpiler: new HarmonyMethod(GetType(), nameof(ForceKindWords)));
            harm.Patch(AccessTools.Method(typeof(ThoughtHandler), nameof(ThoughtHandler.OpinionOffsetOfGroup)), postfix: new HarmonyMethod(GetType(), nameof(Double)));
        }

        public static IEnumerable<CodeInstruction> ForceKindWords(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.opcode == OpCodes.Call && ins.operand is MethodInfo mi && mi.Name == "TryRandomElementByWeight") + 2;
            Assert.AreEqual(list[idx1].opcode, OpCodes.Ldarg_0);
            var label1 = generator.DefineLabel();
            list[idx1].labels.Add(label1);
            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerWorker), nameof(HasPower), generics: new[] {typeof(PowerWorker_Celebrity)})),
                new CodeInstruction(OpCodes.Brfalse, label1),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CelebrityDefOf), nameof(CelebrityDefOf.KindWords))),
                new CodeInstruction(OpCodes.Stloc, 4)
            });
            return list;
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
    }
}