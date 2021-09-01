using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace VFEAncients
{
    public class CompQuestOnHacked : ThingComp
    {
        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == CompHackable.HackedSignal && (props as CompProperties_Quest)?.Quest is QuestScriptDef quest)
                QuestUtility.GenerateQuestAndMakeAvailable(quest, new Slate());
        }
    }

    public class CompProperties_Quest : CompProperties
    {
        public QuestScriptDef Quest;
    }
}