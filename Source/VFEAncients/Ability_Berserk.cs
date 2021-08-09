using RimWorld;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VFEAncients
{
    public class Ability_Berserk : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            if (target.HasThing && target.Thing is Pawn)
                target.Pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, true, false, null, false, false, true);
        }
    }
}