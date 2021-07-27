﻿using System.Collections.Generic;
using Verse;

namespace VFEAncients
{
    public class GameComponent_Powers : GameComponent
    {
        public Dictionary<TickerType, List<(Pawn_PowerTracker, PowerDef)>> TickLists = new Dictionary<TickerType, List<(Pawn_PowerTracker, PowerDef)>>
        {
            {TickerType.Normal, new List<(Pawn_PowerTracker, PowerDef)>()},
            {TickerType.Rare, new List<(Pawn_PowerTracker, PowerDef)>()},
            {TickerType.Long, new List<(Pawn_PowerTracker, PowerDef)>()}
        };

        public GameComponent_Powers(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            foreach (var (tracker, power) in TickLists[TickerType.Normal]) power.Worker.Tick(tracker);

            if (Find.TickManager.TicksGame % 250 == 0)
                foreach (var (tracker, power) in TickLists[TickerType.Rare])
                    power.Worker.TickRare(tracker);

            if (Find.TickManager.TicksGame % 2000 == 0)
                foreach (var (tracker, power) in TickLists[TickerType.Long])
                    power.Worker.TickLong(tracker);
        }
    }
}