﻿using UnityEngine;
using Verse;

namespace VFEAncients
{
    public class LaserEyeBeam : Projectile_Explosive
    {
        public override void Draw()
        {
        }

        public override void Impact(Thing hitThing)
        {
            base.Impact(null);
            var graphic = (LaserEyeBeamDraw) ThingMaker.MakeThing(VFEA_DefOf.VFEA_LaserEyeBeamDraw);
            graphic.Setup(launcher, origin, destination);
            GenSpawn.Spawn(graphic, ExactPosition.ToIntVec3(), launcher.Map);
        }
    }

    [StaticConstructorOnStartup]
    internal class LaserEyeBeamDraw : ThingWithComps
    {
        private const int LIFETIME = 30;

        private static readonly Material BeamMat =
            MaterialPool.MatFrom("Projectile/LaserEyeBeam", ShaderDatabase.TransparentPostLight);

        private Vector3 a;
        private Vector3 b;
        private Matrix4x4 drawMatrix;
        private int ticksRemaining;

        public void Setup(Thing launcher, Vector3 origin, Vector3 destination)
        {
            a = origin;
            b = destination;
            ticksRemaining = LIFETIME;
            drawMatrix.SetTRS((a + b) / 2 + Vector3.up * AltitudeLayer.MoteOverhead.AltitudeFor(),
                Quaternion.LookRotation(b - a), new Vector3(5f, 1f, (b - a).magnitude));
            GetComp<CompAffectsSky>()?.StartFadeInHoldFadeOut(10, LIFETIME / 2 - 10, LIFETIME / 2);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
                drawMatrix.SetTRS((a + b) / 2 + Vector3.up * AltitudeLayer.MoteOverhead.AltitudeFor(),
                    Quaternion.LookRotation(b - a), new Vector3(5f, 1f, (b - a).magnitude));
        }

        public override void Tick()
        {
            ticksRemaining--;
            if (ticksRemaining <= 0) Destroy();
        }

        public override void Draw()
        {
            Graphics.DrawMesh(MeshPool.plane10, drawMatrix,
                FadedMaterialPool.FadedVersionOf(BeamMat, Mathf.Sin((float) ticksRemaining / LIFETIME * Mathf.PI)), 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
            Scribe_Values.Look(ref a, "a");
            Scribe_Values.Look(ref b, "b");
        }
    }
}