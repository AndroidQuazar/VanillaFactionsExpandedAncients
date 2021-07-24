using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEAncients
{
    [StaticConstructorOnStartup]
    public class ITab_Pawn_Powers : ITab
    {
        public ITab_Pawn_Powers()
        {
            labelKey = "VFEAncients.TabPowers";
            size = new Vector2(550f, 350f);
        }

        private Pawn_PowerTracker SelPowerTracker => SelPawn?.GetPowerTracker();
        public override bool IsVisible => SelPowerTracker?.AllPowers.Any() ?? false;

        protected override void FillTab()
        {
            SelPowerTracker.AllPowers.Split(out var superpowers, out var weaknesses, def => def.powerType == PowerType.Superpower);
            var rect = new Rect(0, 0, size.x, size.y).ContractedBy(10f);
            DoPowersList(rect.TopHalf(), superpowers, PowerType.Superpower);
            DoPowersList(rect.BottomHalf(), weaknesses, PowerType.Weakness);
        }

        public void DoPowersList(Rect inRect, List<PowerDef> powers, PowerType type)
        {
            Widgets.Label(inRect.TopPartPixels(30f), (type == PowerType.Superpower ? "VFEAncients.Superpowers".Translate() : "VFEAncients.Weaknesses".Translate()) + ":");
            for (var i = 0; i < 5; i++)
            {
                var rect = new Rect(inRect.x + 20f + 100f * i, inRect.y + 40f, 80f, 80f);
                DoEmptyRect(rect, type);
                if (i < powers.Count) DoPowerIcon(rect, powers[i]);
            }
        }

        public void DoEmptyRect(Rect inRect, PowerType type)
        {
            var color = GUI.color;
            if (type == PowerType.Weakness) GUI.color = new Color(0.6f, 0f, 0f);
            GUI.DrawTexture(inRect, ColonistBar.BGTex);
            GUI.color = color;
        }

        public void DoPowerIcon(Rect inRect, PowerDef power)
        {
            GUI.DrawTexture(inRect, power.Icon);
            TooltipHandler.TipRegion(inRect, new TipSignal($"{power.LabelCap}\n\n{power.description}\n{power.Worker.EffectString()}"));
            Widgets.DrawHighlightIfMouseover(inRect);
        }
    }
}