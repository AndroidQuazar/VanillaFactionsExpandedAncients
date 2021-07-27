using System.Linq;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Claws : PowerWorker
    {
        public PowerWorker_Claws(PowerDef def) : base(def)
        {
        }

        public override void Notify_Added(Pawn_PowerTracker parent)
        {
            base.Notify_Added(parent);
            foreach (var part in parent.Pawn.RaceProps.body.AllParts.Where(part => part.def == BodyPartDefOf.Hand))
                parent.Pawn.health.AddHediff(ClawsDefOf.VFEA_PlasteelClaw, part);
        }

        public override void Notify_Removed(Pawn_PowerTracker parent)
        {
            base.Notify_Removed(parent);
            parent.Pawn.health.hediffSet.hediffs.RemoveAll(hediff => hediff.def == ClawsDefOf.VFEA_PlasteelClaw);
        }
    }

    [DefOf]
    public class ClawsDefOf
    {
        public static HediffDef VFEA_PlasteelClaw;
    }
}