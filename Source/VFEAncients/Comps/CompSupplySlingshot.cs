using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class CompSupplySlingshot : ThingComp
    {
        private CompTransporter cachedCompTransporter;

        public CompTransporter Transporter => cachedCompTransporter ??= parent.GetComp<CompTransporter>();

        public virtual int TicksToReturn => 60000 * 7;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;

            if (Transporter.LoadingInProgressOrReadyToLaunch)
                yield return new Command_Action
                {
                    defaultLabel = "VFEAncients.LaunchSupplies".Translate(),
                    icon = CompLaunchable.LaunchCommandTex,
                    alsoClickIfOtherInGroupClicked = false,
                    action = delegate
                    {
                        if (Transporter.AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                "ConfirmSendNotCompletelyLoadedPods".Translate(Transporter.FirstThingLeftToLoadInGroup.LabelCapNoCount, Transporter.FirstThingLeftToLoadInGroup),
                                TryLaunch));
                            return;
                        }

                        TryLaunch();
                    }
                };
        }

        public void TryLaunch()
        {
            if (!parent.Spawned)
            {
                Log.Error("Tried to launch " + parent + ", but it's unspawned.");
                return;
            }

            var transportersInGroup = Transporter.TransportersInGroup(parent.Map);
            if (transportersInGroup == null)
            {
                Log.Error("Tried to launch " + parent + ", but it's not in any group.");
                return;
            }

            if (!Transporter.LoadingInProgressOrReadyToLaunch) return;
            Transporter.TryRemoveLord(parent.Map);
            foreach (var compTransporter in transportersInGroup.ListFullCopy())
            {
                Current.Game.GetComponent<GameComponent_Ancients>().SlingshotQueue.Enqueue(new GameComponent_Ancients.SlingshotInfo
                {
                    Cell = parent.Position,
                    Map = parent.Map,
                    ReturnTick = Find.TickManager.TicksGame + TicksToReturn,
                    Wealth = compTransporter.innerContainer.Sum(item => item.MarketValue * item.stackCount)
                });
                compTransporter.innerContainer.ClearAndDestroyContents();
                compTransporter.CancelLoad(parent.Map);
                SkyfallerMaker.SpawnSkyfaller(VFEA_DefOf.VFEA_SupplyCrateLeaving, compTransporter.parent.Position, parent.Map);
            }

            CameraJumper.TryHideWorld();
        }
    }
}