using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEAncients
{
    public class DebugActionsPowers
    {
        [DebugAction("Pawns", "Give power...", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GivePower()
        {
            var list = new List<DebugMenuOption>
            {
                new DebugMenuOption("*Fill", DebugMenuOptionMode.Tool, delegate
                {
                    foreach (var tracker in from p in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>()
                        let tracker = p.GetPowerTracker()
                        where tracker != null
                        select tracker)
                        for (var i = 0; i < 5; i++)
                        {
                            if (DefDatabase<PowerDef>.AllDefs.Where(power => power.powerType == PowerType.Superpower && !tracker.HasPower(power)).TryRandomElement(out var super))
                                tracker.AddPower(super);
                            if (DefDatabase<PowerDef>.AllDefs.Where(power => power.powerType == PowerType.Weakness && !tracker.HasPower(power)).TryRandomElement(out var weak))
                                tracker.AddPower(weak);
                        }
                })
            };
            list.AddRange(DefDatabase<PowerDef>.AllDefs.Select(def => new DebugMenuOption(def.label, DebugMenuOptionMode.Tool, delegate
            {
                foreach (var tracker in from p in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>()
                    let tracker = p.GetPowerTracker()
                    where tracker != null
                    select tracker) tracker.AddPower(def);
            })));

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }
}