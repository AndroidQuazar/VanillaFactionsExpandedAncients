using Verse;

namespace VFEAncients
{
    public class HediffComp_Phasing : HediffComp
    {
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            parent.pawn.pather.TryRecoverFromUnwalkablePosition(false);
        }
    }
}