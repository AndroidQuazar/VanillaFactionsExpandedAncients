using RimWorld;
using Verse;

namespace VFEAncients
{
    public class Ability_Jump : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            var map = Caster.Map;
            var flyer = (SuperJumpingPawn) PawnFlyer.MakeFlyer(VFEA_DefOf.VFEA_SuperJumpingPawn, CasterPawn, target.Cell);
            flyer.ability = this;
            flyer.target = target.CenterVector3;
            GenSpawn.Spawn(flyer, Caster.Position, map);

            base.Cast(target);
        }
    }
}