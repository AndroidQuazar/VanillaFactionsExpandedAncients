using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEAncients
{
    public class JobGiver_ZealotExecution : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return pawn.Map.mapPawns.AllPawnsSpawned.Where(p => p.IsPrisoner && p.guest.HostFaction == pawn.Faction)
                .Select(p => JobMaker.MakeJob(ZealotDefOf.VFEA_ZealotExecution, p)).FirstOrDefault();
        }
    }

    [DefOf]
    public class ZealotDefOf
    {
        public static JobDef VFEA_ZealotExecution;
        public static PowerDef Zealot;
    }
}