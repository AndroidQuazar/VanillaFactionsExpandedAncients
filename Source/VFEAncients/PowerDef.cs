using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace VFEAncients
{
    public class PowerDef : Def
    {
        public List<AbilityDef> abilities;
        public List<HediffDef> hediffs;
        public PowerType powerType;
        public List<StatModifier> statFactors = new List<StatModifier>();
        public List<StatModifier> statOffsets = new List<StatModifier>();
        public string texPath;
        public Type workerClass = typeof(PowerWorker);

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
            LongEventHandler.ExecuteWhenFinished(() => Icon = ContentFinder<Texture2D>.Get(texPath));
        }
    }

    public enum PowerType
    {
        Superpower,
        Weakness
    }

    public class PowerWorker
    {
        public PowerDef def;

        public PowerWorker(PowerDef def)
        {
            this.def = def;
        }

        public virtual void Notify_Added(Pawn_PowerTracker parent)
        {
            if (def.abilities != null)
                foreach (var ability in def.abilities)
                    parent.Pawn.abilities.GainAbility(ability);
            if (def.hediffs != null)
                foreach (var hediff in def.hediffs)
                    parent.Pawn.health.AddHediff(hediff);
        }

        public virtual void Notify_Removed(Pawn_PowerTracker parent)
        {
            if (def.abilities != null)
                foreach (var ability in def.abilities)
                    parent.Pawn.abilities.RemoveAbility(ability);
            if (def.hediffs != null)
                foreach (var hediff in parent.Pawn.health.hediffSet.hediffs.Where(hd => hd.Part == null && def.hediffs.Contains(hd.def)).ToList())
                    parent.Pawn.health.RemoveHediff(hediff);
        }
    }
}