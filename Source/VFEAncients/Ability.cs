using Verse;

namespace VFEAncients
{
    public class Ability : VFECore.Abilities.Ability
    {
        public override Command_Action GetGizmo()
        {
            return new Command_AbilityCooldown(pawn, this);
        }
    }
}