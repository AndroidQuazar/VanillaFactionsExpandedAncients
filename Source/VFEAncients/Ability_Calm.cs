using RimWorld;
using Verse;

namespace VFEAncients
{
    public class Ability_Calm : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            target.Pawn?.MentalState?.RecoverFromState();
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages) || !target.HasThing || !(target.Thing is Pawn p)) return false;
            if (p.InMentalState) return true;
            if (showMessages) Messages.Message("VFEAncients.NotInMentalState".Translate(), MessageTypeDefOf.RejectInput);
            return false;
        }
    }
}