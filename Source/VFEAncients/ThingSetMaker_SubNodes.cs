﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class ThingSetMaker_SubNodes : ThingSetMaker
    {
        public List<ThingSetMaker> subNodes;

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            outThings.AddRange(subNodes.SelectMany(maker => maker.Generate(parms)));
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return subNodes.SelectMany(maker => maker.AllGeneratableThingsDebug(parms));
        }

        protected override bool CanGenerateSub(ThingSetMakerParams parms)
        {
            return base.CanGenerateSub(parms) && subNodes.All(maker => maker.CanGenerate(parms));
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            foreach (var maker in subNodes) maker.ResolveReferences();
        }
    }
}