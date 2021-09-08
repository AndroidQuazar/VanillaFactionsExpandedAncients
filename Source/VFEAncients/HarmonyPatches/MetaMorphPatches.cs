using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VFEAncients.HarmonyPatches
{
    public static class MetaMorphPatches
    {
        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.ExposeData)), postfix: new HarmonyMethod(typeof(MetaMorphPatches), nameof(SaveMetamorphed)));
            harm.Patch(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.HealthScale)), new HarmonyMethod(typeof(MetaMorphPatches), nameof(MetaMorphHealth)));
            harm.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt)),
                transpiler: new HarmonyMethod(typeof(MetaMorphPatches), nameof(InsertCheckMetaMorphForDraw)));
            harm.Patch(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.VerbProperties)), new HarmonyMethod(typeof(MetaMorphPatches), nameof(MetaMorphAttacks)));
            harm.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderCache)),
                new HarmonyMethod(typeof(MetaMorphPatches), nameof(CheckMetaMorphForDrawPortrait)));
        }

        public static void SaveMetamorphed(Pawn __instance)
        {
            var metamorped = HediffComp_MetaMorph.MetamorphedPawns.Contains(__instance);
            Scribe_Values.Look(ref metamorped, "metamorphed");
            if (Scribe.mode == LoadSaveMode.LoadingVars && metamorped) HediffComp_MetaMorph.MetamorphedPawns.Add(__instance);
        }

        public static bool MetaMorphHealth(Pawn __instance, ref float __result)
        {
            if (!HediffComp_MetaMorph.MetamorphedPawns.Contains(__instance)) return true;
            var comp = __instance.health.hediffSet.GetAllComps().OfType<HediffComp_MetaMorph>().First();
            __result = comp.Target.RaceProps.lifeStageAges.Last().def.healthScaleFactor * comp.Target.RaceProps.baseHealthScale;
            return false;
        }

        public static void CheckMetaMorphForDraw(Pawn pawn, ref bool useCache, ref PawnRenderFlags pawnRenderFlags)
        {
            if (HediffComp_MetaMorph.MetamorphedPawns.Contains(pawn))
            {
                useCache = false;
                pawnRenderFlags = PawnRenderFlags.None | PawnRenderFlags.HeadStump;
            }
        }

        public static void CheckMetaMorphForDrawPortrait(Pawn ___pawn, ref bool renderHead, ref bool renderBody, ref bool renderHeadgear, ref bool renderClothes)
        {
            if (HediffComp_MetaMorph.MetamorphedPawns.Contains(___pawn))
            {
                renderHead = false;
                renderHeadgear = false;
                renderClothes = false;
            }
        }

        public static IEnumerable<CodeInstruction> InsertCheckMetaMorphForDraw(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_3) + 1;
            if (idx1 <= 0)
            {
                Log.Warning("[VFEAncients] Could not find insertion instruction in InsertCheckMetaMorphForDraw");
                return list;
            }

            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")),
                new CodeInstruction(OpCodes.Ldloca, 3),
                new CodeInstruction(OpCodes.Ldloca, 1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MetaMorphPatches), nameof(CheckMetaMorphForDraw)))
            });
            return list;
        }

        public static bool MetaMorphAttacks(Pawn __instance, ref List<VerbProperties> __result)
        {
            if (!HediffComp_MetaMorph.MetamorphedPawns.Contains(__instance)) return true;
            var comp = __instance.health.hediffSet.GetAllComps().OfType<HediffComp_MetaMorph>().First();
            __result = comp.Target.race.Verbs;
            return false;
        }
    }
}