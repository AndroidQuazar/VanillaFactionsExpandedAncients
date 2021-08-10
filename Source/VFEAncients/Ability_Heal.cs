using System.Linq;
using Verse;

namespace VFEAncients
{
    public class Ability_Heal : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            if (target.Pawn != null)
                foreach (var injury in target.Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList())
                    target.Pawn.health.RemoveHediff(injury);
        }
    }
}