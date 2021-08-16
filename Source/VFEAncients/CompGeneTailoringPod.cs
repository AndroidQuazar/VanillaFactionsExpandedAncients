using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class CompGeneTailoringPod : ThingComp, IThingHolder, ISuspendableThingHolder, IThingHolderWithDrawnPawn
    {
        private readonly ThingOwner innerContainer;

        public CompGeneTailoringPod()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public bool IsContentsSuspended => true;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public float HeldPawnDrawPos_Y { get; }
        public float HeldPawnBodyAngle { get; }
        public PawnPosture HeldPawnPosture { get; }
    }
}