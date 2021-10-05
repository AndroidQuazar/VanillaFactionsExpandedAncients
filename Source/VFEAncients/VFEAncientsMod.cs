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
            // Harmony.DEBUG = true;
            PowerPatches.Do(Harm);
            AbilityPatches.Do(Harm);
            PhasingPatches.Do(Harm);
            BuildingPatches.Do(Harm);
            PreceptPatches.Do(Harm);
            PointDefensePatches.Do(Harm);
            MetaMorphPatches.Do(Harm);
            StorytellerPatches.Do(Harm);
        }
    }
}