using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    public class CompQuestOnHacked : ThingComp
    {
        private bool hacked;
        public CompProperties_Quest Props => props as CompProperties_Quest;

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == CompHackable.HackedSignal && Props?.Quest is { } script)
            {
                var quest = QuestUtility.GenerateQuestAndMakeAvailable(script, new Slate());
                if (!quest.hidden && quest.root.sendAvailableLetter) QuestUtility.SendLetterQuestAvailable(quest);
                hacked = true;
                if (Rand.Chance(Props.RaidChance)) IncidentDefOf.RaidEnemy.Worker.TryExecute(StorytellerUtility.DefaultParmsNow(IncidentDefOf.RaidEnemy.category, parent.Map));
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hacked, "questHacked");
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (hacked && Props.hackedGraphic is {Graphic: { } graphic} data)
            {
                var mesh = graphic.MeshAt(parent.Rotation);
                var drawPos = parent.DrawPos;
                drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Graphics.DrawMesh(mesh, drawPos + data.drawOffset.RotatedBy(parent.Rotation), Quaternion.identity,
                    data.Graphic.MatAt(parent.Rotation), 0);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Faction.IsPlayer && Faction.OfPlayer.def == VFEA_DefOf.VFEA_NewVault)
            {
                var faction = parent.Faction;
                foreach (var thing in parent.Map.listerThings.AllThings.Where(t => t.Faction == faction)) thing.SetFactionDirect(Faction.OfPlayer);
            }
        }
    }

    public class CompProperties_Quest : CompProperties
    {
        public GraphicData hackedGraphic;
        public QuestScriptDef Quest;
        public float RaidChance;
    }
}