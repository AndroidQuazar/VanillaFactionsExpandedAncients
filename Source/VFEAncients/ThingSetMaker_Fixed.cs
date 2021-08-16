using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class ThingSetMaker_Fixed : ThingSetMaker
    {
        public int count;
        public IntRange countRange;

        public ThingDef def;

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            var t = ThingMaker.MakeThing(def);
            t.stackCount = count == 0 ? countRange.RandomInRange : count;
            outThings.Add(t);
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return Gen.YieldSingle(def);
        }
    }
}