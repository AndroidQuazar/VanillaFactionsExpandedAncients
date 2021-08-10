using RimWorld;
using Verse;

namespace VFEAncients
{
    public class Ability_Animal : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (target.Pawn == null) return false;
            if (!target.Pawn.AnimalOrWildMan())
            {
                if (showMessages) Messages.Message("VFEAncients.NotAnimal".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            if (target.Pawn.Faction == pawn.Faction)
            {
                if (showMessages) Messages.Message("VFEAncients.Tame".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            return true;
        }

        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            if (!(target.HasThing && target.Thing is Pawn)) return;
            if (!target.Pawn.AnimalOrWildMan()) return;
            if (target.Pawn.MentalState != null &&
                (target.Pawn.MentalState.def == MentalStateDefOf.Manhunter || target.Pawn.MentalState.def == MentalStateDefOf.ManhunterPermanent))
                target.Pawn.MentalState.RecoverFromState();
            else
                InteractionWorker_RecruitAttempt.DoRecruit(pawn, target.Pawn);
        }
    }
}