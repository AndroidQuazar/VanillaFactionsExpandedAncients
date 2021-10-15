using System.Collections.Generic;
using Verse;

namespace VFEAncients
{
    public class Ability_Explode : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            if (def.TryGetModExtension<AbilityExtension_Explosion>(out var ext))
                GenExplosion.DoExplosion(ext.onCaster ? pawn.Position : target.Cell, pawn.Map, ext.explosionRadius, ext.explosionDamageDef, pawn, ext.explosionDamageAmount,
                    ignoredThings: new List<Thing> {pawn});
        }
    }

    public class AbilityExtension_Explosion : DefModExtension
    {
        public int explosionDamageAmount;
        public DamageDef explosionDamageDef;
        public float explosionRadius;
        public bool onCaster;
    }
}