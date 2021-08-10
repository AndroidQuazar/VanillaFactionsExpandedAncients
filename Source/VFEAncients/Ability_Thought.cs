using RimWorld;
using Verse;

namespace VFEAncients
{
    public class Ability_Thought : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            if (def.TryGetModExtension<AbilityExtension_Thought>(out var ext))
            {
                if (ext.casterThought != null) pawn.needs.mood?.thoughts.memories.TryGainMemoryFast(ext.casterThought);
                if (ext.targetThought != null) target.Pawn?.needs.mood?.thoughts.memories.TryGainMemoryFast(ext.targetThought);
            }
        }
    }

    public class AbilityExtension_Thought : DefModExtension
    {
        public ThoughtDef casterThought;
        public ThoughtDef targetThought;
    }
}