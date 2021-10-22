using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VFEAncients
{
    public class Ability_GloryKill : Ability
    {
        public IEnumerable<Pawn> PossibleTargets => GenAdj.CellsAdjacent8Way(pawn).SelectMany(c => c.GetThingList(pawn.Map)).Where(t => t.HostileTo(pawn)).OfType<Pawn>();

        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            if (PossibleTargets.TryRandomElement(out var pawn))
            {
                VFEA_DefOf.VFEA_GloryKill_Music.PlayOneShot(pawn);
                var limbs = AllLimbs(pawn).ToList();
                foreach (var part in limbs.InRandomOrder().Take(new IntRange(1, limbs.Count - 1).RandomInRange)) pawn.health.AddHediff(HediffDefOf.MissingBodyPart, part);

                var blood = new IntRange(25, 50).RandomInRange;
                for (var i = 0; i < blood; i++)
                    FilthMaker.TryMakeFilth(GenRadial.RadialCellsAround(pawn.PositionHeld, 2.9f, true).RandomElement(), pawn.MapHeld, pawn.RaceProps.BloodDef,
                        pawn.LabelIndefinite());

                pawn.Kill(null);
            }
        }

        public override bool IsEnabledForPawn(out string reason)
        {
            if (!base.IsEnabledForPawn(out reason)) return false;
            if (PossibleTargets.Any()) return true;
            reason = "VFEAncients.NoTargets".Translate();
            return false;
        }

        public static IEnumerable<BodyPartRecord> AllLimbs(Pawn pawn) =>
            pawn.health.hediffSet.GetNotMissingParts(tag: BodyPartTagDefOf.MovingLimbCore)
                .Concat(pawn.health.hediffSet.GetNotMissingParts(tag: BodyPartTagDefOf.ManipulationLimbCore));
    }
}