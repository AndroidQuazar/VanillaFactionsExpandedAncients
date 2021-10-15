using RimWorld;
using Verse;
using VFECore.Abilities;

namespace VFEAncients
{
    public class Ability_Jump : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            var map = Caster.Map;
            var flyer = (AbilityPawnFlyer) PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, CasterPawn, target.Cell);
            flyer.ability = this;
            flyer.target = target.CenterVector3;
            GenSpawn.Spawn(flyer, Caster.Position, map);

            base.Cast(target);
        }
    }
}