using Verse;
using VFECore.Abilities;

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
            return base.ValidateTarget(target, showMessages) && target.HasThing && target.Thing is Pawn p && p.InMentalState;
        }
    }
}