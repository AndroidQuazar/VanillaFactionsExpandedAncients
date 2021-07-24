using System.Collections.Generic;
using Verse;

namespace VFEAncients
{
    public class Pawn_PowerTracker : IExposable
    {
        private static readonly Dictionary<Pawn, Pawn_PowerTracker> TRACKERS = new Dictionary<Pawn, Pawn_PowerTracker>();
        private HashSet<PowerDef> powers = new HashSet<PowerDef>();

        public Pawn_PowerTracker(Pawn pawn)
        {
            Pawn = pawn;
        }

        public Pawn Pawn { get; }
        public IEnumerable<PowerDef> AllPowers => powers;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref powers, "powers", LookMode.Def);
        }

        public bool HasPower(PowerDef power)
        {
            return powers.Contains(power);
        }

        public void AddPower(PowerDef power)
        {
            if (powers.Contains(power)) Log.Warning($"Attempted to add power that {Pawn} already has: {power}");
            powers.Add(power);
            power.Worker.Notify_Added(this);
        }

        public void RemovePower(PowerDef power)
        {
            if (!powers.Contains(power)) return;
            powers.Remove(power);
            power.Worker.Notify_Removed(this);
        }

        public static void Save(Pawn __instance)
        {
            var tracker = Get(__instance);
            Scribe_Deep.Look(ref tracker, "vfea_powerTracker", __instance);
            TRACKERS[__instance] = tracker;
        }

        public static Pawn_PowerTracker Get(Pawn pawn)
        {
            if (TRACKERS.TryGetValue(pawn, out var tracker)) return tracker;
            if (CanGetPowers(pawn))
            {
                tracker = new Pawn_PowerTracker(pawn);
                TRACKERS.Add(pawn, tracker);
                return tracker;
            }

            return null;
        }

        public static bool CanGetPowers(Pawn pawn)
        {
            return pawn.RaceProps.intelligence >= Intelligence.Humanlike;
        }
    }
}