using HarmonyLib;
using Verse;
using VFEAncients.HarmonyPatches;

namespace VFEAncients
{
    public class VFEAncientsMod : Mod
    {
        public static Harmony Harm;

        public static bool YayosCombat;

        public VFEAncientsMod(ModContentPack content) : base(content)
        {
            if (ModLister.HasActiveModWithName("Yayo's Combat 3 [Adopted]")) YayosCombat = true;
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