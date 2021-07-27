using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Resurrect : PowerWorker
    {
        private static readonly Dictionary<Pawn, int> resses = new Dictionary<Pawn, int>();

        public PowerWorker_Resurrect(PowerDef def) : base(def)
        {
        }

        public override void DoPatches(Harmony harm)
        {
            base.DoPatches(harm);
            harm.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)), postfix: new HarmonyMethod(GetType(), nameof(Notify_Died)));
        }

        public static void Notify_Died(Pawn __instance)
        {
            if (HasPower<PowerWorker_Resurrect>(__instance)) resses.Add(__instance, Find.TickManager.TicksGame + 180000); // 3 days
        }

        public override void Tick(Pawn_PowerTracker parent)
        {
            base.Tick(parent);
            if (resses.ContainsKey(parent.Pawn) && Find.TickManager.TicksGame >= resses[parent.Pawn])
            {
                if (!parent.Pawn.Destroyed)
                {
                    ResurrectionUtility.Resurrect(parent.Pawn);
                    if (PawnUtility.ShouldSendNotificationAbout(parent.Pawn))
                        Messages.Message("MessagePawnResurrected".Translate(parent.Pawn), parent.Pawn, MessageTypeDefOf.PositiveEvent);
                }

                resses.Remove(parent.Pawn);
            }
        }
    }
}