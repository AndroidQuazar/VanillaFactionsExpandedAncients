using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using VFECore.Abilities;

namespace VFEAncients.HarmonyPatches
{
    public static class AbilityPatches
    {
        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)), postfix: new HarmonyMethod(typeof(AbilityPatches), nameof(AddGizmos)));
        }

        public static IEnumerable<Gizmo> AddGizmos(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            foreach (var gizmo in gizmos) yield return gizmo;

            if (__instance.IsColonist && !__instance.IsColonistPlayerControlled && __instance.TryGetComp<CompAbilities>(out var comp))
                foreach (var ability in comp.LearnedAbilities.Where(ab => ab is IForceGizmo))
                    if (ability.ShowGizmoOnPawn())
                        yield return ability.GetGizmo();
        }
    }

    public interface IForceGizmo
    {
        bool ShowGizmoOnPawn();
    }
}