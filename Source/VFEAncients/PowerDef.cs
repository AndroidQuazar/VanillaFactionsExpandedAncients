using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

// ReSharper disable InconsistentNaming

namespace VFEAncients
{
    public class PowerDef : Def
    {
        private static readonly List<Type> appliedPatches = new List<Type>();
        public List<AbilityDef> abilities;
        public string effectDescription;
        public List<HediffDef> hediffs;
        public List<ThoughtDef> nullifiedThoughts;
        public PowerType powerType;
        public List<StatModifier> statFactors = new List<StatModifier>();
        public List<StatModifier> statOffsets = new List<StatModifier>();
        public string texPath;
        public TickerType tickerType = TickerType.Never;
        public Type workerClass = typeof(PowerWorker);
        public WorkerData workerData;

        public Texture2D Icon { get; private set; }

        public PowerWorker Worker { get; private set; }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors()) yield return error;

            if (workerClass == null) yield return "Worker class cannot be null";
            if (!typeof(PowerWorker).IsAssignableFrom(workerClass)) yield return "Worker class must be subclass of PowerWorker";
        }

        public override void PostLoad()
        {
            base.PostLoad();
            Worker = (PowerWorker) Activator.CreateInstance(workerClass, this);
            if (!appliedPatches.Contains(workerClass))
            {
                Worker.DoPatches(VFEAncientsMod.Harm);
                appliedPatches.Add(workerClass);
            }

            LongEventHandler.ExecuteWhenFinished(() => Icon = ContentFinder<Texture2D>.Get(texPath));
        }
    }

    public class WorkerData
    {
    }

    public enum PowerType
    {
        Superpower,
        Weakness
    }

    public class PowerWorker
    {
        public PowerDef def;

        private bool donePatches;

        public PowerWorker(PowerDef def)
        {
            this.def = def;
        }

        protected T GetData<T>() where T : WorkerData
        {
            return def.workerData as T;
        }

        public virtual void Notify_Added(Pawn_PowerTracker parent)
        {
            if (def.abilities != null && parent.Pawn.TryGetComp<CompAbilities>(out var comp))
                foreach (var ability in def.abilities)
                    comp.GiveAbility(ability);
            if (def.hediffs != null)
                foreach (var hediff in def.hediffs)
                    parent.Pawn.health.AddHediff(hediff);
            if (def.tickerType != TickerType.Never) Current.Game.GetComponent<GameComponent_Powers>().TickLists[def.tickerType].Add((parent, def));
        }

        public virtual void Notify_Removed(Pawn_PowerTracker parent)
        {
            if (def.abilities != null && parent.Pawn.TryGetComp<CompAbilities>(out var comp))
                foreach (var ability in def.abilities)
                    comp.LearnedAbilities.RemoveAll(ab => ab.def == ability);
            if (def.hediffs != null)
                foreach (var hediff in parent.Pawn.health.hediffSet.hediffs.Where(hd => hd.Part == null && def.hediffs.Contains(hd.def)).ToList())
                    parent.Pawn.health.RemoveHediff(hediff);
            if (def.tickerType != TickerType.Never) Current.Game.GetComponent<GameComponent_Powers>().TickLists[def.tickerType].Remove((parent, def));
        }

        public string EffectString()
        {
            var builder = new StringBuilder();
            if (def.abilities != null)
                foreach (var ability in def.abilities)
                    builder.AppendLine("VFEAncients.Effect.AddAbility".Translate(ability.label));
            if (def.hediffs != null)
                foreach (var hediff in def.hediffs)
                    builder.AppendLine("VFEAncients.Effect.AddHediff".Translate(hediff.label));
            if (def.statFactors != null)
                foreach (var factor in def.statFactors)
                    builder.AppendLine($"{factor.stat.LabelForFullStatListCap}: {factor.ToStringAsFactor}");
            if (def.statOffsets != null)
                foreach (var factor in def.statOffsets)
                    builder.AppendLine($"{factor.stat.LabelForFullStatListCap}: {factor.ValueToStringAsOffset}");
            if (def.nullifiedThoughts != null)
                foreach (var thoughtDef in def.nullifiedThoughts)
                    builder.AppendLine("VFEAncients.Effect.NullThought".Translate(thoughtDef.Label));
            if (!AdditionalEffects().NullOrEmpty()) builder.AppendLine(AdditionalEffects());
            if (!def.effectDescription.NullOrEmpty()) builder.AppendLine(def.effectDescription);
            if (builder.Length > 0) builder.Insert(0, "VFEAncients.Effects".Translate() + "\n");
            return builder.ToString();
        }

        public virtual void DoPatches(Harmony harm)
        {
            if (!donePatches)
            {
                harm.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.ThoughtNullified)),
                    postfix: new HarmonyMethod(GetType(), nameof(ThoughtNullified_Postfix)));
                harm.Patch(AccessTools.Method(typeof(ThoughtUtility), nameof(ThoughtUtility.ThoughtNullifiedMessage)),
                    postfix: new HarmonyMethod(GetType(), nameof(ThoughtNullifiedMessage_Postfix)));
                donePatches = true;
            }
        }

        public static void ThoughtNullified_Postfix(Pawn pawn, ThoughtDef def, ref bool __result)
        {
            if (!__result && NullifyingPower(def, pawn) != null) __result = true;
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

        public virtual string AdditionalEffects()
        {
            return "";
        }

        public static bool HasPower<T>(Thing caster) where T : PowerWorker
        {
            return caster is Pawn pawn && (pawn.GetPowerTracker()?.AllPowers.Any(power => power?.Worker is T) ?? false);
        }

        public static WorkerData GetData<T>(Thing caster) where T : WorkerData
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