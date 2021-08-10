using HarmonyLib;
using Verse;
using VFEAncients.HarmonyPatches;

namespace VFEAncients
{
    public class VFEAncientsMod : Mod
    {
        public static ModSettings Settings;
        public static Harmony Harm;

        public VFEAncientsMod(ModContentPack content) : base(content)
        {
            Harm = new Harmony("VanillaExpanded.VFEA");
            PowerPatches.Do(Harm);
            ElectricPatches.Do(Harm);
            AbilityPatches.Do(Harm);
        }
    }
}