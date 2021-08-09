using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    public class CompSolarPowerUp : ThingComp
    {
        private bool oldElectricityDisabled;
        private CompPowerTrader powerComp;

        public CompProperties_SolarPowerUp Props => props as CompProperties_SolarPowerUp;

        public static bool PowerUpActive(Thing parent)
        {
            return parent.Map.GameConditionManager.ElectricityDisabled;
        }

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
                else
                {
                    powerComp.SetUpPowerVars();
                }
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

        public CompProperties_SolarPowerUp()
        {
            compClass = typeof(CompSolarPowerUp);
        }
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

    [StaticConstructorOnStartup]
    public class CompAncientSolar : CompPowerPlantSolar
    {
        private static readonly Vector2 BAR_SIZE = new Vector2(2.3f, 0.14f);

        private static readonly Material POWER_PLANT_SOLAR_BAR_FILLED_MAT = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

        private static readonly Material POWER_PLANT_SOLAR_BAR_UNFILLED_MAT = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));
        protected override float DesiredPowerOutput => Mathf.Lerp(0f, -Props.basePowerConsumption, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;

        private float RoofedPowerOutputFactor
        {
            get
            {
                var cells = parent.OccupiedRect().ToList();
                return cells.Count(cell => !parent.Map.roofGrid.Roofed(cell)) / (float) cells.Count;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = BAR_SIZE;
            r.fillPercent = Mathf.Clamp(PowerOutput / -Props.basePowerConsumption, 0f, 1f);
            r.filledMat = POWER_PLANT_SOLAR_BAR_FILLED_MAT;
            r.unfilledMat = POWER_PLANT_SOLAR_BAR_UNFILLED_MAT;
            r.margin = 0.15f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }
}