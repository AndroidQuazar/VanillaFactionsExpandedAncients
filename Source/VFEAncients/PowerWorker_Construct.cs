﻿using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Construct : PowerWorker
    {
        public PowerWorker_Construct(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(Frame), nameof(Frame.CompleteConstruction)), new HarmonyMethod(GetType(), nameof(Refund)));
        }

        public static void Refund(Frame __instance, Pawn worker)
        {
            if (HasPower<PowerWorker_Construct>(worker) &&
                worker.GetPowerTracker().AllPowers.First(p => p.Worker is PowerWorker_Construct).workerData is WorkerData_Construct data && Rand.Chance(data.RefundChance))
                if (data.RefundAmount >= 1f)
                    __instance.resourceContainer.TryDropAll(__instance.Position, __instance.Map, ThingPlaceMode.Near);
                else
                    foreach (var thing in __instance.resourceContainer.ToList())
                        if (thing.stackCount > 1)
                            __instance.resourceContainer.TryDrop(thing, ThingPlaceMode.Near, Mathf.CeilToInt(thing.stackCount * data.RefundAmount), out _);
                        else if (Rand.Chance(data.RefundAmount)) __instance.resourceContainer.TryDrop(thing, ThingPlaceMode.Near, out _);
        }
    }

    public class WorkerData_Construct : WorkerData
    {
        public float RefundAmount;
        public float RefundChance;
    }
}