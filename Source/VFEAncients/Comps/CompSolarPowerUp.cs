using RimWorld;
using Verse;

namespace VFEAncients
{
    public class CompSolarPowerUp : ThingComp
    {
        private bool oldElectricityDisabled;
        private CompPowerTrader powerComp;

        public CompProperties_SolarPowerUp Props => props as CompProperties_SolarPowerUp;

        public static bool PowerUpActive(Thing parent) => parent.Spawned && parent.Map.GameConditionManager.ElectricityDisabled;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.TryGetComp<CompPowerTrader>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (powerComp != null)
                if (PowerUpActive(parent))
                {
                    powerComp.PowerOutput = Props.PowerOutputSolarFlare;
                    powerComp.PowerOn = true;
                }
                else if (powerComp is CompPowerPlant plant)
                    plant.UpdateDesiredPowerOutput();
                else
                    powerComp.SetUpPowerVars();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref oldElectricityDisabled, "oldElectricityDisabled");
        }
    }

    public class CompProperties_SolarPowerUp : CompProperties
    {
        public float PowerOutputSolarFlare;
        public float SolarFlareWorkSpeedMult = 2f;

        public CompProperties_SolarPowerUp() => compClass = typeof(CompSolarPowerUp);
    }

    public class StatPart_SolarPowerUp : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.Map.GameConditionManager.ElectricityDisabled && req.Thing.TryGetComp<CompSolarPowerUp>(out var solarPowerUp))
                val *= solarPowerUp.Props.SolarFlareWorkSpeedMult;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Map.GameConditionManager.ElectricityDisabled && req.Thing.TryGetComp<CompSolarPowerUp>(out var solarPowerUp))
                return "VFEAncients.SolarPowerUp".Translate() + ": x" + solarPowerUp.Props.SolarFlareWorkSpeedMult.ToStringPercent();
            return "";
        }
    }
}