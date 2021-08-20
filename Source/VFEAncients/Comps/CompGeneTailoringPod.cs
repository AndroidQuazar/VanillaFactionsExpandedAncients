﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEAncients
{
    [StaticConstructorOnStartup]
    public class CompGeneTailoringPod : ThingComp, IThingHolder, ISuspendableThingHolder, IThingHolderWithDrawnPawn
    {
        private static readonly Material BackgroundMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.082f, 0.078f, 0.063f));
        private readonly List<Operation> possibleOperations;
        private Operation currentOperation;
        private ThingOwner innerContainer;

        private int ticksTillDone = -1;

        public CompGeneTailoringPod()
        {
            innerContainer = new ThingOwner<Thing>(this);
            possibleOperations = typeof(Operation).AllSubclassesNonAbstract().Select(opType => (Operation) Activator.CreateInstance(opType, this)).ToList();
        }

        public bool PowerOn => parent.GetComp<CompPowerTrader>().PowerOn;
        public bool HasFuel => parent.GetComp<CompRefuelable>().HasFuel;

        public Pawn Occupant => innerContainer.OfType<Pawn>().FirstOrDefault();

        public bool IsContentsSuspended => true;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public float HeldPawnDrawPos_Y => parent.DrawPos.y - Altitudes.AltInc;
        public float HeldPawnBodyAngle => parent.Rotation.Opposite.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        public virtual void StartOperation(Operation op)
        {
            currentOperation = op;
            parent.GetComp<CompRefuelable>().ConsumeFuel(1f);
            ticksTillDone = currentOperation.StartOnPawnGetDuration();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ticksTillDone > 0 && currentOperation != null)
            {
                ticksTillDone--;
                if (!PowerOn)
                {
                    currentOperation.Failure();
                    currentOperation = null;
                    ticksTillDone = -1;
                }

                if (ticksTillDone == 0) CompleteOperation();
            }
        }

        public virtual void CompleteOperation()
        {
            if (Rand.Chance(currentOperation.FailChanceOnPawn(Occupant)))
                currentOperation.Failure();
            else currentOperation.Success();
            currentOperation = null;
            ticksTillDone = -1;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize) EjectContents(previousMap);

            innerContainer.ClearAndDestroyContents();
            base.PostDestroy(mode, previousMap);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Occupant != null && HasFuel && PowerOn && ticksTillDone <= 0 && currentOperation == null)
                yield return new Command_Action
                {
                    action = () => Find.WindowStack.Add(new FloatMenu(possibleOperations.Where(op => op.CanRunOnPawn(Occupant))
                        .Select(op => new FloatMenuOption(op.Label, () => StartOperation(op))).ToList())),
                    defaultLabel = "VFEAncients.StartOperation".Translate(),
                    icon = Texture2D.normalTexture
                };
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.IsQuestLodger())
                yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
            else if (!selPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            else if (CanAccept(selPawn))
                yield return FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption("VFEAncients.EnterGeneTailoringPod".Translate(),
                        () => { selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VFEA_DefOf.VFEA_EnterGeneTailoringPod, parent), JobTag.Misc); }), selPawn, parent);
        }

        public static void AddCarryToPodJobs(List<FloatMenuOption> opts, Pawn pawn, Pawn target)
        {
            if (!pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true)) return;
            foreach (var thing in FindPodsFor(pawn, target))
            {
                string text = "VFEAncients.CarryToGeneTailoringPod".Translate(target);
                if (target.IsQuestLodger())
                {
                    text += " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")";
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, null), pawn, thing));
                }
                else if (target.GetExtraHostFaction() != null)
                {
                    text += " (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")";
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, null), pawn, thing));
                }
                else if (thing.TryGetComp<CompGeneTailoringPod>().CanAccept(target))
                {
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, () =>
                    {
                        var job = JobMaker.MakeJob(VFEA_DefOf.VFEA_CarryToGeneTailoringPod, target, thing);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }), pawn, thing));
                }
            }
        }

        public static IEnumerable<Thing> FindPodsFor(Pawn pawn, Pawn target)
        {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.comps.Any(comp => comp.compClass == typeof(CompGeneTailoringPod))).Select(podDef =>
                GenClosest.ClosestThingReachable(target.Position, pawn.Map, ThingRequest.ForDef(podDef), PathEndMode.InteractionCell,
                    TraverseParms.For(pawn), 9999f, pod => pod.TryGetComp<CompGeneTailoringPod>().CanAccept(pawn))).Where(thing => thing != null);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            var s = new Vector3(parent.def.graphicData.drawSize.x * 0.6f, 1f, parent.def.graphicData.drawSize.y * 0.6f);
            var drawPos = parent.DrawPos;
            drawPos.y -= Altitudes.AltInc * 2;
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos, parent.Rotation.AsQuat, s), BackgroundMat, 0);
            if (Occupant != null)
            {
                var drawLoc = parent.DrawPos;
                drawLoc.y -= Altitudes.AltInc;
                if (parent.Rotation == Rot4.South) drawLoc.z -= 0.1f;
                if (parent.Rotation == Rot4.West || parent.Rotation == Rot4.East) drawLoc.z += 0.1f;
                if (parent.Rotation == Rot4.West) drawLoc.x -= 0.3f;
                if (parent.Rotation == Rot4.East) drawLoc.x += 0.3f;
                Occupant.Drawer.renderer.RenderPawnAt(drawLoc, parent.Rotation, true);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            currentOperation?.Failure();
            EjectContents(map);
        }

        public bool CanAccept(Pawn pawn)
        {
            return PowerOn && HasFuel && Pawn_PowerTracker.CanGetPowers(pawn) && Occupant == null && possibleOperations.Any(op => op.CanRunOnPawn(pawn));
        }

        public void TryAcceptPawn(Pawn pawn)
        {
            if (!CanAccept(pawn)) return;

            if (pawn.Spawned) pawn.DeSpawn();

            if (pawn.holdingOwner != null)
                pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
            else
                innerContainer.TryAdd(pawn);
        }

        public void EjectContents(Map destMap = null)
        {
            if (destMap == null) destMap = parent.Map;
            innerContainer.TryDropAll(parent.InteractionCell, destMap ?? parent.Map, ThingPlaceMode.Near);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref currentOperation, "currentOperation", this);
            Scribe_Values.Look(ref ticksTillDone, "ticksTillDone");
            Scribe_Deep.Look(ref innerContainer, "innerContainer");
        }

        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLineIfNotEmpty().Append(base.CompInspectStringExtra()).AppendLineIfNotEmpty();
            if (parent.Spawned)
            {
                if (Occupant != null) stringBuilder.AppendLineIfNotEmpty().Append("Contains".Translate()).Append(": ").Append(Occupant.NameShortColored.Resolve());

                if (currentOperation != null)
                    stringBuilder.AppendLineIfNotEmpty().Append("VFEAncients.CurrentOperation".Translate()).Append(": ").Append(currentOperation.Label);
                if (ticksTillDone > 0)
                    stringBuilder.AppendLineIfNotEmpty().Append("BiosculpterCycleTimeRemaining".Translate()).Append(": ")
                        .Append(ticksTillDone.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor));
            }

            return stringBuilder.Length <= 0 ? null : stringBuilder.ToString();
        }
    }
}