using RimWorld;
using Verse;

namespace VFEAncients
{
    internal class ThoughtWorker_OutsideParanoid : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return (p.GetPowerTracker()?.HasPower(ParanoidDefOf.Paranoid) ?? false) && !p.Map.areaManager.Home[p.Position];
        }
    }

    [DefOf]
    public class ParanoidDefOf
    {
        public static PowerDef Paranoid;
    }
}