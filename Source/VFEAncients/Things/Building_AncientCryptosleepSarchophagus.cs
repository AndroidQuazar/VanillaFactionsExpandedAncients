using RimWorld;
using Verse;

namespace VFEAncients
{
    public class Building_AncientCryptosleepSarchophagus : Building_AncientCryptosleepCasket
    {
        private CompHackable compHackable;
        private bool hasOpened;
        public override bool CanOpen => base.CanOpen && compHackable.IsHacked;
        public override bool Accepts(Thing thing) => base.Accepts(thing) && !hasOpened;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compHackable = this.TryGetComp<CompHackable>();
        }

        public override void Open()
        {
            base.Open();
            hasOpened = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hasOpened, "hasOpened");
        }
    }
}