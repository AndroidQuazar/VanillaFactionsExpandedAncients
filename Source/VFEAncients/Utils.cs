using System;
using System.Collections.Generic;
using Verse;

namespace VFEAncients
{
    public static class Utils
    {
        public static Pawn_PowerTracker GetPowerTracker(this Pawn pawn)
        {
            return Pawn_PowerTracker.Get(pawn);
        }

        public static void Split<T>(this IEnumerable<T> source, out List<T> truthy, out List<T> falsy, Func<T, bool> func)
        {
            truthy = new List<T>();
            falsy = new List<T>();
            foreach (var t in source)
                if (func(t))
                    truthy.Add(t);
                else falsy.Add(t);
        }

        public static bool TryGetComp<T>(this Thing t, out T comp) where T : ThingComp
        {
            comp = t.TryGetComp<T>();
            return comp != null;
        }
    }
}