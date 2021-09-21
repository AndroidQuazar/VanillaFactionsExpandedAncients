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
    }

    public class CompProperties_Quest : CompProperties
    {
        public GraphicData hackedGraphic;
        public QuestScriptDef Quest;
    }
}