using Verse;
using Verse.AI;

namespace VFEAncients
{
    public class ThinkNode_ConditionalPower : ThinkNode_Conditional
    {
        public PowerDef Power;

        protected override bool Satisfied(Pawn pawn)
        {
            return pawn?.GetPowerTracker()?.HasPower(Power) ?? false;
        }
    }
}