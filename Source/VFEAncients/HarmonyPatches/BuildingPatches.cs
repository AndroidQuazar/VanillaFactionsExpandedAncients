using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEAncients.HarmonyPatches
{
    public static class BuildingPatches
    {
        public static void Do(Harmony harm)
        {
            harm.Patch(typeof(WorkGiver_DoBill).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .FirstOrDefault(cls => cls.Name.Contains("20_0"))?.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(method => method.GetParameters().Any(parm => parm.ParameterType == typeof(Thing))),
                transpiler: new HarmonyMethod(typeof(BuildingPatches), nameof(ExtraValidation)));
            harm.Patch(AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet"),
                postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(TryFindStuffIngredients)));
            harm.Patch(AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)), new HarmonyMethod(typeof(BuildingPatches), nameof(RepairItem)));
            harm.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(AddCarryJobs)));
            harm.Patch(AccessTools.Method(typeof(SteadyEnvironmentEffects), nameof(SteadyEnvironmentEffects.FinalDeteriorationRate),
                    new[] {typeof(Thing), typeof(bool), typeof(bool), typeof(bool), typeof(TerrainDef), typeof(List<string>)}),
                postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(AddDeterioration)));
            harm.Patch(AccessTools.Method(typeof(JobDriver_Hack), "MakeNewToils"), postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(FixHacking)));
        }

        public static IEnumerable<Toil> FixHacking(IEnumerable<Toil> toils, JobDriver_Hack __instance)
        {
            var done = __instance.job.targetA.Thing.def.hasInteractionCell;
            foreach (var toil in toils)
            {
                if (!done)
                {
                    toil.initAction = delegate { toil.actor.pather.StartPath(toil.actor.jobs.curJob.GetTarget(TargetIndex.A), PathEndMode.Touch); };
                    done = true;
                }

                yield return toil;
            }
        }

        public static void AddDeterioration(Thing t, List<string> reasons, ref float __result)
        {
            if (t.TryGetComp<CompNeedsContainment>(out var comp) && comp.ShouldDeteriorate)
            {
                __result += t.GetStatValue(StatDefOf.DeteriorationRate);
                reasons?.Add("VFEAncients.DeterioratingUncontained".Translate());
            }
        }

        public static void AddCarryJobs(List<FloatMenuOption> opts, Vector3 clickPos, Pawn pawn)
        {
            foreach (var localTargetInfo4 in GenUI.TargetsAt(clickPos, TargetingParameters.ForCarryToBiosculpterPod(pawn), true))
            {
                var target = localTargetInfo4.Pawn;
                if (target.IsColonist && target.Downed || target.IsPrisonerOfColony) CompGeneTailoringPod.AddCarryToPodJobs(opts, pawn, target);
            }

            foreach (var info in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn)))
            {
                var target = info.Pawn;
                if (target.Downed) CompBioBattery.AddCarryToBatteryJobs(opts, pawn, target);
            }
        }

        public static IEnumerable<CodeInstruction> ExtraValidation(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.opcode == OpCodes.Bge_Un_S);
            var label1 = generator.DefineLabel();
            var label2 = generator.DefineLabel();
            var label3 = (Label) list[idx1].operand;
            var label4 = generator.DefineLabel();
            var label5 = generator.DefineLabel();
            idx1++;
            var idx2 = list.FindIndex(idx1, ins => ins.opcode == OpCodes.Call) + 2;
            list[idx1].labels.Add(label2);
            list[idx2].labels.Add(label1);
            var getBill = list[idx1 + 1];
            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                getBill.Clone(),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Bill), nameof(Bill.recipe))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Def), nameof(Def.HasModExtension), generics: new[] {typeof(RecipeExtension_Mend)})),
                new CodeInstruction(OpCodes.Brfalse, label2),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ThingDef), nameof(ThingDef.IsWeapon))),
                new CodeInstruction(OpCodes.Brtrue, label4),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ThingDef), nameof(ThingDef.IsApparel))),
                new CodeInstruction(OpCodes.Brtrue, label4),
                new CodeInstruction(OpCodes.Br, label5),
                new CodeInstruction(OpCodes.Ldarg_1).WithLabels(label4),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(ThingDef.useHitPoints))),
                new CodeInstruction(OpCodes.Brfalse, label3),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.HitPoints))),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.MaxHitPoints))),
                new CodeInstruction(OpCodes.Bge, label3),
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(label5),
                getBill.Clone(),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Bill), nameof(Bill.recipe))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.ingredients))),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<IngredientCount>), "get_Item", new[] {typeof(int)})),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IngredientCount), nameof(IngredientCount.filter))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ThingFilter), nameof(ThingFilter.AllowedThingDefs))),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingPatches), nameof(IsStuffIngredient))),
                new CodeInstruction(OpCodes.Call, typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(meth => meth.Name == "Any" && meth.GetParameters().Length == 2)
                    ?.MakeGenericMethod(typeof(ThingDef))),
                new CodeInstruction(OpCodes.Brtrue, label1)
            });
            return list;
        }

        public static bool RepairItem(RecipeDef recipeDef, List<Thing> ingredients, ref IEnumerable<Thing> __result)
        {
            if (recipeDef.HasModExtension<RecipeExtension_Mend>())
            {
                var item = ingredients.FirstOrDefault(t => t.stackCount == 1 && (t.def.IsWeapon || t.def.IsApparel) && t.def.useHitPoints && t.HitPoints < t.MaxHitPoints);
                if (item != null)
                {
                    item.HitPoints = item.MaxHitPoints;
                    __result = Gen.YieldSingle(item);
                    return false;
                }
            }

            return true;
        }

        public static Func<ThingDef, bool> IsStuffIngredient(Thing t)
        {
            return def => def.MadeFromStuff
                ? t.def.stuffProps?.CanMake(def) ?? false
                : NonStuffStuff(def) == t.def;
        }

        public static ThingDef NonStuffStuff(ThingDef def)
        {
            return def.CostList != null && def.CostList.Count > 0
                ? def.CostListAdjusted(null).MaxBy(tdcc => tdcc.count).thingDef
                : StuffFromTech(def.techLevel);
        }

        public static ThingDef StuffFromTech(TechLevel level)
        {
            switch (level)
            {
                case TechLevel.Animal:
                case TechLevel.Neolithic:
                case TechLevel.Medieval:
                    return ThingDefOf.WoodLog;
                case TechLevel.Undefined:
                case TechLevel.Industrial:
                    return ThingDefOf.Steel;
                case TechLevel.Spacer:
                case TechLevel.Ultra:
                case TechLevel.Archotech:
                    return ThingDefOf.Plasteel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public static void TryFindStuffIngredients(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, ref bool __result)
        {
            if (__result && bill.recipe.HasModExtension<RecipeExtension_Mend>())
            {
                var tc = chosen[0];
                if (tc.Count == 1)
                {
                    var item = tc.Thing;
                    var chosenDef = item.Stuff ?? NonStuffStuff(item.def);
                    var countWanted = Mathf.RoundToInt(Mathf.Clamp((item.Stuff != null
                                                                       ? item.def.CostStuffCount
                                                                       : item.def.CostList != null && item.def.CostList.Any()
                                                                           ? item.CostListAdjusted().First(tdcc => tdcc.thingDef == chosenDef).count
                                                                           : item.GetStatValue(StatDefOf.MarketValueIgnoreHp) / 100) *
                                                                   bill.recipe.GetModExtension<RecipeExtension_Mend>().Fraction, 1f, 9999f));
                    foreach (var thing in availableThings.Where(thing => thing.def == chosenDef))
                    {
                        chosen.Add(new ThingCount(thing, countWanted));
                        countWanted -= thing.stackCount;
                        if (countWanted <= 0) return;
                    }

                    __result = false;
                }
            }
        }
    }
}