using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients.HarmonyPatches
{
    public class PointDefensePatches
    {
        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Building_TurretGun), "TryStartShootSomething"), new HarmonyMethod(typeof(PointDefensePatches), nameof(TryShootProjectile)));
            harm.Patch(AccessTools.Method(typeof(Projectile), nameof(Projectile.Launch),
                new[]
                {
                    typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef)
                }), new HarmonyMethod(typeof(PointDefensePatches), nameof(PreLaunch)));
            harm.Patch(AccessTools.Method(typeof(Projectile), "ImpactSomething"), new HarmonyMethod(typeof(PointDefensePatches), nameof(PreImpactSomething)));
        }

        public static bool TryShootProjectile(Building_TurretGun __instance, ref LocalTargetInfo ___currentTargetInt, bool canBeginBurstImmediately,
            ref int ___burstWarmupTicksLeft)
        {
            if (__instance.def == VFEA_DefOf.VFEA_Turret_AncientPointDefense)
                if (__instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile)
                    .Where(t => t.Position.InHorDistOf(__instance.Position, __instance.AttackVerb.verbProps.range)).TryRandomElement(out var thing))
                {
                    ___currentTargetInt = thing;
                    if (__instance.def.building.turretBurstWarmupTime > 0f)
                    {
                        ___burstWarmupTicksLeft = __instance.def.building.turretBurstWarmupTime.SecondsToTicks();
                        return false;
                    }

                    ___burstWarmupTicksLeft = 1;
                    return false;
                }

            return true;
        }

        public static void PreLaunch(ref LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, Projectile __instance)
        {
            if (intendedTarget.Thing is Projectile proj && Rand.Chance(0.75f) && proj.Spawned) usedTarget = proj;
        }

        public static bool PreImpactSomething(Projectile __instance)
        {
            if (__instance.usedTarget.Thing is Projectile proj && proj.Spawned)
            {
                GenClamor.DoClamor(__instance, 12f, ClamorDefOf.Impact);
                __instance.Destroy();
                proj.Destroy();
                return false;
            }

            return true;
        }
    }
}