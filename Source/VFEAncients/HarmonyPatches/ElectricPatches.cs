﻿using HarmonyLib;
using RimWorld;

namespace VFEAncients.HarmonyPatches
{
    public static class ElectricPatches
    {
        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.PropertySetter(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOn)),
                new HarmonyMethod(typeof(ElectricPatches), nameof(PowerOn_Prefix)));
            harm.Patch(AccessTools.PropertySetter(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOutput)),
                new HarmonyMethod(typeof(ElectricPatches), nameof(PowerOutput_Prefix)));
        }

        public static void PowerOn_Prefix(CompPowerTrader __instance, ref bool value)
        {
            if (CompSolarPowerUp.PowerUpActive(__instance.parent) && __instance.parent.TryGetComp<CompSolarPowerUp>(out _)) value = true;
        }

        public static void PowerOutput_Prefix(CompPowerTrader __instance, ref float value)
        {
            if (CompSolarPowerUp.PowerUpActive(__instance.parent) && __instance.parent.TryGetComp<CompSolarPowerUp>(out var comp)) value = comp.Props.PowerOutputSolarFlare;
        }
    }
}