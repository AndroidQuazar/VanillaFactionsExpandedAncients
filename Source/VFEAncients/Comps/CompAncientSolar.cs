﻿using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    [StaticConstructorOnStartup]
    public class CompAncientSolar : CompPowerPlantSolar
    {
        private static readonly Vector2 BAR_SIZE = new(2.3f, 0.14f);

        private static readonly Material POWER_PLANT_SOLAR_BAR_FILLED_MAT = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));
        private static readonly Material POWER_PLANT_SOLAR_BAR_SUPER_MAT = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.45f, 0.1f, 0.9f));

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
            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = BAR_SIZE;
            r.fillPercent = PowerOutput / -Props.basePowerConsumption;
            r.filledMat = POWER_PLANT_SOLAR_BAR_FILLED_MAT;
            r.unfilledMat = POWER_PLANT_SOLAR_BAR_UNFILLED_MAT;
            if (r.fillPercent > 1f)
            {
                r.fillPercent = 1f;
                r.filledMat = POWER_PLANT_SOLAR_BAR_SUPER_MAT;
            }

            r.margin = 0.15f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }
}