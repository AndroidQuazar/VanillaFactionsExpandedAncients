using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class GenStep_PopulateVault : GenStep
    {
        public override int SeedPart => 981491759;

        public override void Generate(Map map, GenStepParams parms)
        {
            foreach (var casket in map.listerThings.AllThings.OfType<Building_CryptosleepCasket>().ToList())
            {
                casket.ContainedThing?.Destroy();
                casket.EjectContents();
                casket.TryAcceptThing(PawnGenerator.GeneratePawn(
                    casket.Faction?.IsPlayer ?? false ? VFEA_DefOf.VFEA_PlayerAncientSoldierOneAbility :
                    Rand.Bool ? VFEA_DefOf.VFEA_AncientSoldierOneAbility : VFEA_DefOf.VFEA_AncientSoldierTwoAbilities, casket.Faction));
                Traverse.Create(casket).Field("contentsKnown").SetValue(false);
            }

            foreach (var crate in map.listerThings.AllThings.OfType<Building_Crate>().ToList())
            {
                crate.ContainedThing?.Destroy();
                crate.EjectContents();
                var things = ThingSetMakerDefOf.MapGen_AncientComplex_SecurityCrate.root.Generate(default);
                foreach (var thing in things)
                {
                    if (crate.def == VFEA_DefOf.VFEA_AncientSupplyCrate) thing.stackCount *= 2;
                    crate.TryAcceptThing(thing, false);
                }

                Traverse.Create(crate).Field("contentsKnown").SetValue(false);
            }
        }
    }
}