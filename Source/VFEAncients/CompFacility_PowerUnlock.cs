using RimWorld;

namespace VFEAncients
{
    public class CompFacility_PowerUnlock : CompFacility
    {
    }

    public class CompProperties_Facility_PowerUnlock : CompProperties_Facility
    {
        public int unlockedLevel;

        public CompProperties_Facility_PowerUnlock()
        {
            compClass = typeof(CompFacility_PowerUnlock);
        }
    }
}