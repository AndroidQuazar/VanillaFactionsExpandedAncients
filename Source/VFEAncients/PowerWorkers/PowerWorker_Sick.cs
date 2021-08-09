using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFEAncients
{
    public class PowerWorker_Sick : PowerWorker
    {
        public PowerWorker_Sick(PowerDef def) : base(def)
        {
        }

        public override void TickLong(Pawn_PowerTracker parent)
        {
            base.TickLong(parent);
            if (Rand.Chance(0.05f))
            {
                var incident = DefDatabase<IncidentDef>.AllDefs.Where(d => d.Worker is IncidentWorker_DiseaseHuman).RandomElement();
                var pawn = parent.Pawn;
                var text = "";
                if (Rand.Chance(pawn.health.immunity.DiseaseContractChanceFactor(incident.diseaseIncident, out var hediffDef)))
                {
                    HediffGiverUtility.TryApply(pawn, incident.diseaseIncident, incident.diseasePartsToAffect);
                    TaleRecorder.RecordTale(TaleDefOf.IllnessRevealed, pawn, incident.diseaseIncident);
                    text += string.Format(incident.letterText, 1.ToString(), Faction.OfPlayer.def.pawnSingular, incident.diseaseIncident.label, $"  - {pawn.LabelNoCountColored}");
                }
                else if (hediffDef != null)
                {
                    text += "LetterDisease_Blocked".Translate(hediffDef.LabelCap, incident.diseaseIncident.label, pawn.LabelNoCountColored);
                }

                var letter = LetterMaker.MakeLetter(incident.letterLabel, text, incident.letterDef, new LookTargets(Gen.YieldSingle(pawn)));
                letter.hyperlinkHediffDefs = new List<HediffDef> {incident.diseaseIncident};
                Find.LetterStack.ReceiveLetter(letter);
            }
        }
    }
}