using Verse;

namespace VFEAncients
{
    public class Ability_CalmSelf : Ability
    {
        public override bool ShowGizmoOnPawn() => base.ShowGizmoOnPawn() || pawn.Faction.IsPlayer && pawn.InMentalState;

        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            pawn.MentalState?.RecoverFromState();
        }

        public override bool IsEnabledForPawn(out string reason)
        {
            if (!base.IsEnabledForPawn(out reason)) return false;
            if (pawn.InMentalState) return true;

            reason = "VFEAncients.NotInMentalState".Translate();
            return false;
        }
    }
}