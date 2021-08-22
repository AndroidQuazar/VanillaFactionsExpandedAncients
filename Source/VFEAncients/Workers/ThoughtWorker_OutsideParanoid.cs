using RimWorld;
using Verse;

namespace VFEAncients
{
    internal class ThoughtWorker_OutsideParanoid : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return (p.GetPowerTracker()?.HasPower(VFEA_DefOf.Paranoid) ?? false) && !(p.MapHeld?.areaManager.Home[p.Position] ?? false);
        }
    }
}