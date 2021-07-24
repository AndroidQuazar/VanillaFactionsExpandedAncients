using RimWorld;
using Verse;

namespace VFEAncients
{
    public class CompSolarPowerUp : ThingComp
    {
        private float oldPowerOutput;
        private CompPowerTrader powerComp;

        public CompProperties_SolarPowerUp Props => props as CompProperties_SolarPowerUp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.TryGetComp<CompPowerTrader>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Map.GameConditionManager.ElectricityDisabled)
            {
                powerComp.PowerOn = true;
                if (powerComp.PowerOutput != Props.PowerOutputSolarFlare)
                {
                    oldPowerOutput = powerComp.PowerOutput;
                    powerComp.PowerOutput = Props.PowerOutputSolarFlare;
                }
            }
            else if (powerComp.PowerOutput == Props.PowerOutputSolarFlare)
            {
                powerComp.PowerOutput = oldPowerOutput;
            }
        }
    }

    public class CompProperties_SolarPowerUp : CompProperties
    {
        public float PowerOutputSolarFlare;
        public float SolarFlareWorkSpeedMult = 2f;

        public CompProperties_SolarPowerUp()
        {
            compClass = typeof(CompSolarPowerUp);
        }
    }

    public class StatPart_SolarPowerUp : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.Map.GameConditionManager.ElectricityDisabled && req.Thing.TryGetComp<CompSolarPowerUp>() is CompSolarPowerUp solarPowerUp)
                val *= solarPowerUp.Props.SolarFlareWorkSpeedMult;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.Map.GameConditionManager.ElectricityDisabled && req.Thing.TryGetComp<CompSolarPowerUp>() is CompSolarPowerUp solarPowerUp)
                return "VFEAncients.SolarPowerUp".Translate() + ": x" + solarPowerUp.Props.SolarFlareWorkSpeedMult.ToStringPercent();
            return "";
        }
    }
}