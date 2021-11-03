using System.Linq;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class Building_AncientCryptosleepSarchophagus : Building_AncientCryptosleepCasket
    {
        private CompHackable compHackable;
        private bool hasOpened;
        public override bool CanOpen => base.CanOpen && compHackable.IsHacked;
        public override bool Accepts(Thing thing) => !hasOpened && base.Accepts(thing);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !hasOpened) hasOpened = true;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            compHackable = this.TryGetComp<CompHackable>();
            if (compHackable is not null) compHackable.progress = compHackable.defence;
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (hasOpened) return false;
            if (compHackable is not null) compHackable.progress = 0f;
            if (!Spawned && thing.Faction is {IsPlayer: true}) thing.SetFactionDirect(Faction.OfAncients);
            return base.TryAcceptThing(thing, allowSpecialEffects);
        }

        public override void Open()
        {
            if (Faction.IsPlayer && ContainedThing is Pawn pawn)
            {
                var pawn2 = Map.mapPawns.FreeColonists.OrderBy(p => p.Position.DistanceTo(Position)).FirstOrDefault();
                if (pawn2 is null) pawn.SetFaction(Faction);
                else InteractionWorker_RecruitAttempt.DoRecruit(pawn2, pawn, out _, out _);
            }

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