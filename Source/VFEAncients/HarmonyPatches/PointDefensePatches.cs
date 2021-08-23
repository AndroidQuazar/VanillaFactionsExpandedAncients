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
            harm.Patch(AccessTools.PropertyGetter(typeof(Building_TurretGun), nameof(Building_TurretGun.AttackVerb)),
                new HarmonyMethod(typeof(PointDefensePatches), nameof(OverrideAttackVerb)));
        }

        public static bool OverrideAttackVerb(Building_TurretGun __instance, ref Verb __result)
        {
            if (__instance.CurrentTarget.Thing is Projectile_Explosive || __instance.CurrentTarget.Thing is DropPodIncoming)
            {
                __result = __instance.GunCompEq.AllVerbs.First(v => v.verbProps.label == "point-defense");
                return false;
            }

            return true;
        }

        private static bool TryFindPDTarget(Building_TurretGun searcher, out LocalTargetInfo target)
        {
            var range = searcher.AttackVerb.verbProps.range;
            target = searcher.Map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile)
                .Where(t => t is Projectile_Explosive && t.HostileTo(searcher) && t.Position.InHorDistOf(searcher.Position, range))
                .OrderByDescending(t => t.Position.DistanceTo(searcher.Position))
                .FirstOrDefault();
            if (target.IsValid) return true;
            target = searcher.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod)
                .Where(t => t is DropPodIncoming && t.HostileTo(searcher) && Mathf.Abs((t.DrawPos - searcher.DrawPos).magnitude) <= range).OrderByDescending(t =>
                    Mathf.Abs((t.DrawPos - searcher.DrawPos).magnitude)).FirstOrDefault();
            if (target.IsValid) return true;
            return false;
        }

        public static bool TryShootProjectile(Building_TurretGun __instance, ref LocalTargetInfo ___currentTargetInt, bool canBeginBurstImmediately,
            ref int ___burstWarmupTicksLeft)
        {
            if (__instance.def == VFEA_DefOf.VFEA_Turret_AncientPointDefense && TryFindPDTarget(__instance, out var target))
            {
                ___currentTargetInt = target;
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
            if (intendedTarget.Thing is Projectile_Explosive proj && Rand.Chance(0.75f) && proj.Spawned) usedTarget = proj;
            if (intendedTarget.Thing is DropPodIncoming pod && pod.Spawned) usedTarget = pod;
        }

        public static bool PreImpactSomething(Projectile __instance)
        {
            var flag = false;
            switch (__instance.usedTarget.Thing)
            {
                case Projectile_Explosive proj when proj.Spawned:
                    proj.Destroy();
                    break;
                case DropPodIncoming pod when pod.Spawned:
                    pod.Destroy();
                    break;
                default:
                    flag = true;
                    break;
            }

            if (!flag)
            {
                GenClamor.DoClamor(__instance, 12f, ClamorDefOf.Impact);
                __instance.Destroy();
            }

            return flag;
        }
    }
}