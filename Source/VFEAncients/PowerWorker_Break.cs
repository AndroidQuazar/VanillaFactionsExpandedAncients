using Verse;

namespace VFEAncients
{
    public class PowerWorker_Break : PowerWorker
    {
        public PowerWorker_Break(PowerDef def) : base(def)
        {
        }

        public override void TickLong(Pawn_PowerTracker parent)
        {
            base.TickLong(parent);
            if (Rand.Chance(GetData<WorkerData_Break>().BreakChance)) GetData<WorkerData_Break>().Break.Worker.TryStart(parent.Pawn, null, false);
        }
    }
}