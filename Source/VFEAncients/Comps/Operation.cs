using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    public abstract class Operation : IExposable
    {
        public CompGeneTailoringPod Pod;

        protected Operation(CompGeneTailoringPod pod) => Pod = pod;

        public virtual int TicksRequired => Mathf.RoundToInt(7 * 60000 * Pod.parent.GetStatValue(VFEA_DefOf.VFEA_InjectingTimeFactor));

        public abstract string Label { get; }

        public void ExposeData()
        {
        }

        public virtual bool CanRunOnPawn(Pawn pawn) => true;

        public virtual int StartOnPawnGetDuration() => TicksRequired;

        public virtual float FailChanceOnPawn(Pawn pawn)
        {
            return pawn.GetPowerTracker().HasPower(VFEA_DefOf.PromisingCandidate)
                ? 0f
                : Pod.parent.GetStatValue(VFEA_DefOf.VFEA_FailChance) + pawn.GetPowerTracker().AllPowers.Count(power => power.powerType == PowerType.Superpower) * 0.1f;
        }

        public virtual void Failure()
        {
            var failType = typeof(Fail).AllSubclassesNonAbstract().RandomElement();
            var fail = (Fail) Activator.CreateInstance(failType);
            var pawn = Pod.Occupant;
            Pod.EjectContents();
            fail.RunOnPawn(pawn);
        }

        public abstract void Success();
    }

    public class Operation_Empower : Operation
    {
        public Operation_Empower(CompGeneTailoringPod pod) : base(pod)
        {
        }

        public override string Label => "VFEAncients.Empower".Translate();

        public virtual int MaxPowerLevel =>
            Pod.parent.TryGetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.OfType<ThingWithComps>().SelectMany(t => t.AllComps)
                .Select(comp => comp.props)
                .OfType<CompProperties_Facility_PowerUnlock>().Append(new CompProperties_Facility_PowerUnlock {unlockedLevels = 3})
                .Sum(props => props.unlockedLevels);

        public override bool CanRunOnPawn(Pawn pawn)
        {
            return base.CanRunOnPawn(pawn) && pawn.GetPowerTracker().AllPowers.Count(power => power.powerType == PowerType.Superpower) < MaxPowerLevel;
        }

        public override void Success()
        {
            var tracker = Pod.Occupant.GetPowerTracker();
            DefDatabase<PowerDef>.AllDefs.Except(tracker.AllPowers).Split(out var superpowers, out var weaknessess, def => def.powerType == PowerType.Superpower);
            Action<Tuple<PowerDef, PowerDef>> onPowers = powers =>
            {
                tracker.AddPower(powers.Item1);
                tracker.AddPower(powers.Item2);
                Pod.EjectContents();
                Find.LetterStack.ReceiveLetter("VFEAncients.Empowered.Label".Translate(tracker.Pawn.LabelShortCap),
                    "VFEAncients.Empowered.Text".Translate(tracker.Pawn.NameShortColored, powers.Item1.LabelCap, powers.Item2.LabelCap), LetterDefOf.PositiveEvent, tracker.Pawn);
            };
            GenDebug.LogList(Pod.parent.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading);
            if (Pod.parent.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Any(t => t.def == VFEA_DefOf.VFEA_NaniteSampler))
                Find.WindowStack.Add(new Dialog_ChoosePowers(new List<Tuple<PowerDef, PowerDef>>
                {
                    new(superpowers.RandomElement(), weaknessess.RandomElement()),
                    new(superpowers.RandomElement(), weaknessess.RandomElement())
                }, Pod.Occupant, onPowers));
            else
                onPowers(new Tuple<PowerDef, PowerDef>(superpowers.RandomElement(), weaknessess.RandomElement()));
        }
    }

    public class Operation_RemoveWeakness : Operation
    {
        public Operation_RemoveWeakness(CompGeneTailoringPod pod) : base(pod)
        {
        }

        public override string Label => "VFEAncients.RemoveWeakness".Translate();

        public override float FailChanceOnPawn(Pawn pawn)
        {
            if (pawn.GetPowerTracker().HasPower(VFEA_DefOf.PromisingCandidate)) return 0f;
            return base.FailChanceOnPawn(pawn) + 0.5f;
        }

        public override bool CanRunOnPawn(Pawn pawn)
        {
            return base.CanRunOnPawn(pawn) && Pod.parent.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Any(t => t.def == VFEA_DefOf.VFEA_NanotechRetractor) &&
                   pawn.GetPowerTracker().AllPowers.Any(power => power.powerType == PowerType.Weakness);
        }

        public override void Success()
        {
            var tracker = Pod.Occupant.GetPowerTracker();
            var weakness = tracker.AllPowers.LastOrDefault(power => power.powerType == PowerType.Weakness);
            if (weakness == null) return;
            tracker.RemovePower(weakness);
            Pod.EjectContents();
            Find.LetterStack.ReceiveLetter("VFEAncients.RemoveWeakness.Label".Translate(tracker.Pawn.LabelShortCap),
                "VFEAncients.RemoveWeakness.Text".Translate(tracker.Pawn.NameShortColored, weakness.LabelCap), LetterDefOf.PositiveEvent, tracker.Pawn);
        }
    }
}