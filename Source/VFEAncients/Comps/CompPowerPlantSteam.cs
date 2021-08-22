﻿using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    public class CompPowerPlantSteam : CompPowerPlant
    {
        private CompTempControl compTempControl;
        private Building_SteamGeyser geyser;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            geyser = (Building_SteamGeyser) parent.Map.thingGrid.ThingAt(parent.Position, ThingDefOf.SteamGeyser);

            geyser.harvester = (Building) parent;

            compTempControl = parent.GetComp<CompTempControl>();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (PowerOn)
            {
                var pos = parent.OccupiedRect().AdjacentCells.RandomElement();
                var diff = compTempControl.targetTemperature - parent.AmbientTemperature;
                var factor = Mathf.Sign(diff) * Mathf.InverseLerp(0, 5f, Mathf.Abs(diff));
                var energyLimit = compTempControl.Props.energyPerSecond * factor * 4.16666651f;
                var room = pos.GetRoom(parent.Map);
                var change = GenTemperature.ControlTemperatureTempChange(pos, parent.Map, energyLimit, compTempControl.targetTemperature);
                var flag = !Mathf.Approximately(change, 0f);
                if (flag && room != null) room.Temperature += change;

                compTempControl.operatingAtHighPower = flag;
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            geyser.harvester = null;
        }
    }
}