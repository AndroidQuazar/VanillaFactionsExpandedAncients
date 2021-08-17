using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class ThingSetMaker_Minified : ThingSetMaker
    {
        public ThingDef def;

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            var mini = (MinifiedThing) ThingMaker.MakeThing(def.minifiedDef);
            mini.InnerThing = ThingMaker.MakeThing(def);
            outThings.Add(mini);
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return Gen.YieldSingle(def.minifiedDef);
        }
    }
}