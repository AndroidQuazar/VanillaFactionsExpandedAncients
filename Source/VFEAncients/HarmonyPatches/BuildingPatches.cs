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
                transpiler: new HarmonyMethod(typeof(BuildingPatches), nameof(ExtraValidation)), postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(Debug)));
            harm.Patch(AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet"),
                postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(TryFindStuffIngredients)));
            harm.Patch(AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)), new HarmonyMethod(typeof(BuildingPatches), nameof(RepairItem)));
            harm.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(AddCarryJobs)));
            harm.Patch(AccessTools.Method(typeof(SteadyEnvironmentEffects), nameof(SteadyEnvironmentEffects.FinalDeteriorationRate),
                    new[] {typeof(Thing), typeof(bool), typeof(bool), typeof(bool), typeof(TerrainDef), typeof(List<string>)}),
                postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(AddDeterioration)));
            harm.Patch(AccessTools.Method(typeof(JobDriver_Hack), "MakeNewToils"), postfix: new HarmonyMethod(typeof(BuildingPatches), nameof(FixHacking)));
            harm.Patch(AccessTools.Method(typeof(PowerNet), nameof(PowerNet.PowerNetTick)), new HarmonyMethod(typeof(BuildingPatches), nameof(PowerNetOnSolarFlare)));
        }

        private static void Debug(Thing t, ref bool __result)
        {
            // Log.Message($"ValidateIngredient: {t} -> {__result}");
        }

        public static void PowerNetOnSolarFlare(PowerNet __instance)
        {
            if (__instance.Map.GameConditionManager.ElectricityDisabled) __instance.PowerNetTickSolarFlare();
        }

        public static IEnumerable<Toil> FixHacking(IEnumerable<Toil> toils, JobDriver_Hack __instance)
        {
            var idx = 0;
            foreach (var toil in toils)
            {
                if (!__instance.job.targetA.Thing.def.hasInteractionCell)
                    switch (idx)
                    {
                        case 0:
                            toil.initAction = delegate { toil.actor.pather.StartPath(toil.actor.jobs.curJob.GetTarget(TargetIndex.A), PathEndMode.Touch); };
                            break;
                        case 1:
                            toil.endConditions = new List<Func<JobCondition>>
                            {
                                () => toil.actor.CanReachImmediate(toil.actor.jobs.curJob.GetTarget(TargetIndex.A), PathEndMode.Touch)
                                    ? JobCondition.Ongoing
                                    : JobCondition.Incompletable,
                                () => toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing.TryGetComp<CompHackable>().IsHacked ? JobCondition.Succeeded : JobCondition.Ongoing
                            };
                            break;
                    }

                idx++;
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
            var billField = AccessTools.Field(typeof(WorkGiver_DoBill).GetNestedTypes(AccessTools.all)
                .First(c => c.Name.Contains("c__DisplayClass20_0")), "bill");
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, billField);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingPatches), nameof(ExtraVerificationCheck)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static bool ExtraVerificationCheck(Bill bill, Thing t)
        {
            if (bill.recipe is null || !bill.recipe.HasModExtension<RecipeExtension_Mend>())
                return bill.IsFixedOrAllowedIngredient(t) &&
                       bill.recipe.ingredients.Any(ingNeed =>
                           ingNeed.filter.Allows(t));
            Log.Message($"ExtraVerificationCheck: bill={bill}, recipe={bill.recipe}, thing={t}");
            if (!t.def.useHitPoints) return bill.recipe.ingredients[0].filter.AllowedThingDefs.Any(IsStuffIngredient(t));
            Log.Message($"UsesHitPoints: HitPoints={t.HitPoints}, MaxHitPoints={t.MaxHitPoints}, Ret={bill.IsFixedOrAllowedIngredient(t) && t.HitPoints < t.MaxHitPoints}");
            return bill.IsFixedOrAllowedIngredient(t) && t.HitPoints < t.MaxHitPoints;
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