using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using VFECore.Abilities;

namespace VFEAncients
{
    public class PowerWorker
    {
        private static bool donePatches;

        private static readonly FieldInfo permanentOnly =
            AccessTools.Field(AccessTools.FirstInner(typeof(Pawn), type => AccessTools.Field(type, "permanentOnly") is not null), "permanentOnly");

        public PowerDef def;

        public PowerWorker(PowerDef def) => this.def = def;

        public virtual IEnumerable<WorkTypeDef> DisabledWorkTypes => DefDatabase<WorkTypeDef>.AllDefs.Where(wtd => !AllowsWorkType(wtd));

        protected T GetData<T>() where T : WorkerData => def.workerData as T;

        public virtual void Notify_Added(Pawn_PowerTracker parent)
        {
            if (def.abilities != null && parent.Pawn.TryGetComp<CompAbilities>(out var comp))
                foreach (var ability in def.abilities)
                    comp.GiveAbility(ability);
            if (def.hediffs != null)
                foreach (var hediff in def.hediffs)
                    parent.Pawn.health.AddHediff(hediff);
            if (def.tickerType != TickerType.Never) Current.Game.GetComponent<GameComponent_Ancients>().TickLists[def.tickerType].Add((parent, def));
            if (def.disabledWorkTags != WorkTags.None) parent.Pawn.cachedDisabledWorkTypes?.Clear();
        }

        public virtual void Notify_Removed(Pawn_PowerTracker parent)
        {
            if (def.abilities != null && parent.Pawn.TryGetComp<CompAbilities>(out var comp))
                foreach (var ability in def.abilities)
                    comp.LearnedAbilities.RemoveAll(ab => ab.def == ability);
            if (def.hediffs != null)
                foreach (var hediff in parent.Pawn.health.hediffSet.hediffs.Where(hd => hd.Part == null && def.hediffs.Contains(hd.def)).ToList())
                    parent.Pawn.health.RemoveHediff(hediff);
            if (def.tickerType != TickerType.Never) Current.Game.GetComponent<GameComponent_Ancients>().TickLists[def.tickerType].Remove((parent, def));
            if (def.disabledWorkTags != WorkTags.None) parent.Pawn.cachedDisabledWorkTypes?.Clear();
        }

        public string EffectString()
        {
            var builder = new StringBuilder();
            if (def.abilities != null)
                foreach (var ability in def.abilities)
                {
                    builder.AppendLine("VFEAncients.Effect.AddAbility".Translate(ability.label));
                    if (!ability.description.NullOrEmpty()) builder.AppendLine(ability.description);
                }

            if (def.hediffs != null)
                foreach (var hediff in def.hediffs)
                    builder.AppendLine("VFEAncients.Effect.AddHediff".Translate(hediff.label));
            if (def.statFactors != null)
                foreach (var factor in def.statFactors)
                    builder.AppendLine($"{factor.stat.LabelForFullStatListCap}: {factor.ToStringAsFactor}");
            if (def.statOffsets != null)
                foreach (var factor in def.statOffsets)
                    builder.AppendLine($"{factor.stat.LabelForFullStatListCap}: {factor.ValueToStringAsOffset}");
            if (def.setStats != null)
                foreach (var factor in def.setStats)
                    builder.AppendLine($"{factor.stat.LabelForFullStatListCap}: {factor.stat.ValueToString(factor.value)}");
            if (def.nullifiedThoughts != null)
                foreach (var thoughtDef in def.nullifiedThoughts)
                    builder.AppendLine("VFEAncients.Effect.NullThought".Translate(thoughtDef.Label.Formatted("")));
            if (def.disabledWorkTags != WorkTags.None)
                foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefs.Where(work => (work.workTags & def.disabledWorkTags) != WorkTags.None))
                    builder.AppendLine(workTypeDef.gerundLabel.CapitalizeFirst() + " " + "DisabledLower".Translate());
            if (!AdditionalEffects().NullOrEmpty()) builder.AppendLine(AdditionalEffects());
            if (!def.effectDescription.NullOrEmpty()) builder.AppendLine(def.effectDescription);
            if (builder.Length > 0) builder.Insert(0, "\n" + "VFEAncients.Effects".Translate() + "\n");
            return builder.ToString();
        }

        private bool AllowsWorkType(WorkTypeDef workType) => (def.disabledWorkTags & workType.workTags) == WorkTags.None;

        private bool AllowsWorkGiver(WorkGiverDef workGiver) => (def.disabledWorkTags & workGiver.workTags) == WorkTags.None;

        public virtual void DoPatches(Harmony harm)
        {
            if (donePatches) return;
            harm.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.ThoughtNullified)),
                postfix: new HarmonyMethod(GetType(), nameof(ThoughtNullified_Postfix)));
            harm.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.ThoughtNullifiedMessage)),
                postfix: new HarmonyMethod(GetType(), nameof(ThoughtNullifiedMessage_Postfix)));
            harm.Patch(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.CombinedDisabledWorkTags)), postfix: new HarmonyMethod(GetType(), nameof(DisableWork)));
            harm.Patch(AccessTools.Method(typeof(CharacterCardUtility), "GetWorkTypeDisableCauses"), postfix: new HarmonyMethod(GetType(), nameof(AddCauseDisable)));
            harm.Patch(AccessTools.Method(typeof(CharacterCardUtility), "GetWorkTypeDisabledCausedBy"),
                transpiler: new HarmonyMethod(GetType(), nameof(AddCauseDisableExplain)));
            harm.Patch(AccessTools.GetDeclaredMethods(typeof(Pawn)).First(info => info.Name.Contains("GetDisabledWorkTypes") && info.Name.Contains("FillList")),
                postfix: new HarmonyMethod(GetType(), nameof(AddDisabledWorkTypes)));
            donePatches = true;
        }

        public static IEnumerable<CodeInstruction> AddCauseDisableExplain(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.opcode == OpCodes.Ldloca_S);
            var idx2 = list.FindIndex(idx1 + 1, ins => ins.opcode == OpCodes.Ldloca_S);
            var idx3 = list.FindLastIndex(ins => ins.opcode == OpCodes.Brfalse_S);
            var label1 = generator.DefineLabel();
            var label2 = list[idx3].operand;
            list[idx3].operand = label1;
            list.InsertRange(idx2, new[]
            {
                new CodeInstruction(OpCodes.Br, label2),
                new CodeInstruction(OpCodes.Ldloc_2).WithLabels(label1),
                new CodeInstruction(OpCodes.Isinst, typeof(PowerDef)),
                new CodeInstruction(OpCodes.Brfalse, label2),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerWorker), nameof(AddPowerExplain)))
            });
            return list;
        }

        public static void AddPowerExplain(StringBuilder builder, object power)
        {
            builder.AppendLine("VFEAncients.IncapableOfTooltipPower".Translate(((PowerDef) power).label));
        }

        public static void AddCauseDisable(Pawn pawn, WorkTags workTag, ref List<object> __result)
        {
            if (pawn.GetPowerTracker() is { } tracker) __result.AddRange(tracker.AllPowers.Where(power => (power.disabledWorkTags & workTag) != WorkTags.None));
        }

        public static void DisableWork(Pawn __instance, ref WorkTags __result)
        {
            if (__instance.GetPowerTracker() is { } tracker) __result = tracker.AllPowers.Aggregate(__result, (current, power) => current | power.disabledWorkTags);
        }

        public static void AddDisabledWorkTypes(Pawn __instance, List<WorkTypeDef> list, object __1)
        {
            if (__instance.GetPowerTracker() is { } tracker && !(bool) permanentOnly.GetValue(__1))
                foreach (var power in tracker.AllPowers)
                foreach (var workType in power.Worker.DisabledWorkTypes.Except(list))
                    list.Add(workType);
        }

        public static void ThoughtNullified_Postfix(Pawn pawn, ThoughtDef def, ref bool __result)
        {
            if (!__result && pawn != null && NullifyingPower(def, pawn) != null) __result = true;
        }

        public static void ThoughtNullifiedMessage_Postfix(Pawn pawn, ThoughtDef def, ref string __result)
        {
            if (__result.NullOrEmpty() && !ThoughtUtility.NeverNullified(def, pawn))
            {
                var power = NullifyingPower(def, pawn);
                if (power != null) __result = "ThoughtNullifiedBy".Translate().CapitalizeFirst() + ": " + power.LabelCap;
            }
        }

        public static PowerDef NullifyingPower(ThoughtDef def, Pawn pawn)
        {
            return pawn.GetPowerTracker()?.AllPowers.FirstOrDefault(power => power.nullifiedThoughts?.Contains(def) ?? false);
        }

        public virtual string AdditionalEffects() => "";

        public static bool HasPower<T>(Thing caster) where T : PowerWorker
        {
            return caster is Pawn pawn && (pawn.GetPowerTracker()?.AllPowers.Any(power => power?.Worker is T) ?? false);
        }

        public static T GetData<T>(Thing caster) where T : WorkerData
        {
            return (caster as Pawn)?.GetPowerTracker()?.AllPowers.FirstOrDefault(p => p.workerData is T)?.Worker.GetData<T>();
        }

        public virtual void Tick(Pawn_PowerTracker parent)
        {
        }

        public virtual void TickRare(Pawn_PowerTracker parent)
        {
        }

        public virtual void TickLong(Pawn_PowerTracker parent)
        {
        }
    }
}