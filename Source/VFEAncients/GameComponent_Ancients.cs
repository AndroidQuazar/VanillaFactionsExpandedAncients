﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class GameComponent_Ancients : GameComponent
    {
        public Queue<SlingshotInfo> SlingshotQueue = new();

        public Dictionary<TickerType, List<(Pawn_PowerTracker, PowerDef)>> TickLists = new()
        {
            {TickerType.Normal, new List<(Pawn_PowerTracker, PowerDef)>()},
            {TickerType.Rare, new List<(Pawn_PowerTracker, PowerDef)>()},
            {TickerType.Long, new List<(Pawn_PowerTracker, PowerDef)>()}
        };

        public GameComponent_Ancients(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (Find.TickManager.TicksGame % 2000 == 0)
                foreach (var (tracker, power) in TickLists[TickerType.Long])
                    try
                    {
                        power.Worker.TickLong(tracker);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception ticking power {power}: {e}");
                    }

            if (Find.TickManager.TicksGame % 250 == 0)
                foreach (var (tracker, power) in TickLists[TickerType.Rare])
                    try
                    {
                        power.Worker.TickRare(tracker);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception ticking power {power}: {e}");
                    }

            foreach (var (tracker, power) in TickLists[TickerType.Normal])
                try
                {
                    power.Worker.Tick(tracker);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception ticking power {power}: {e}");
                }

            if (SlingshotQueue.TryPeek(out var info) && info.ReturnTick <= Find.TickManager.TicksGame)
            {
                info = SlingshotQueue.Dequeue();
                if (info.Map == null || info.Map.Index < 0)
                {
                    info.Map = Find.AnyPlayerHomeMap;
                    info.Cell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(info.Map, info.Map.Size.ToIntVec2, Faction.OfAncients);
                }

                info.Cell = DropCellFinder.TryFindDropSpotNear(info.Cell, info.Map, out var cell, false, false, false) ? cell : DropCellFinder.TradeDropSpot(info.Map);
                var things = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(new ThingSetMakerParams
                    {podContentsType = PodContentsType.AncientFriendly, totalMarketValueRange = new FloatRange(info.Wealth, info.Wealth)});
                SkyfallerMaker.SpawnSkyfaller(VFEA_DefOf.VFEA_SupplyCrateIncoming, things, info.Cell, info.Map);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            var infos = new List<SlingshotInfo>();
            while (SlingshotQueue.Any()) infos.Add(SlingshotQueue.Dequeue());
            Scribe_Collections.Look(ref infos, "slingshotQueue", LookMode.Deep);
            SlingshotQueue = new Queue<SlingshotInfo>();
            foreach (var info in infos) SlingshotQueue.Enqueue(info);
        }

        public class SlingshotInfo : IExposable
        {
            public IntVec3 Cell;
            public Map Map;
            public int ReturnTick;
            public float Wealth;

            public void ExposeData()
            {
                Scribe_Values.Look(ref ReturnTick, "returnTick");
                Scribe_Values.Look(ref Wealth, "wealth");
                Scribe_References.Look(ref Map, "map");
                Scribe_Values.Look(ref Cell, "cell");
            }
        }
    }
}