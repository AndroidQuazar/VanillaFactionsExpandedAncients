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
        [TweakValue("Vanilla Factions Expanded - Ancients", 1, 60)]
        public static int MaxPowers = 5;

        public bool EditMode;

        private Vector2 scrollPos;

        public ITab_Pawn_Powers()
        {
            labelKey = "VFEAncients.TabPowers";
            size = new Vector2(550f, 350f);
        }

        private Pawn_PowerTracker SelPowerTracker => SelPawn?.GetPowerTracker();
        public override bool IsVisible => (SelPowerTracker?.AllPowers.Any() ?? false) || SelPawn != null && Pawn_PowerTracker.CanGetPowers(SelPawn) && Prefs.DevMode;

        protected override void FillTab()
        {
            SelPowerTracker.AllPowers.Split(out var superpowers, out var weaknesses, def => def.powerType == PowerType.Superpower);
            var rect = new Rect(0, 0, size.x, size.y).ContractedBy(10f);
            Widgets.BeginScrollView(rect, ref scrollPos, new Rect(0, 0, 30f + 100f * MaxPowers, rect.height - 20f));
            DoPowersList(rect.TopHalf(), superpowers, PowerType.Superpower);
            DoPowersList(rect.BottomHalf(), weaknesses, PowerType.Weakness);
            Widgets.EndScrollView();
            if (Prefs.DevMode) Widgets.CheckboxLabeled(new Rect(rect.xMax - 100f, rect.yMin + 10f, 100f, 30f), "Edit mode", ref EditMode);
        }

        public void DoPowersList(Rect inRect, List<PowerDef> powers, PowerType type)
        {
            Widgets.Label(inRect.TopPartPixels(30f), (type == PowerType.Superpower ? "VFEAncients.Superpowers".Translate() : "VFEAncients.Weaknesses".Translate()) + ":");
            for (var i = 0; i < MaxPowers; i++)
            {
                var rect = new Rect(inRect.x + 20f + 100f * i, inRect.y + 40f, 80f, 80f);
                DoEmptyRect(rect, type, i < powers.Count);
                if (i < powers.Count) DoPowerIcon(rect, powers[i]);
            }
        }

        public void DoEmptyRect(Rect inRect, PowerType type, bool filled = false)
        {
            var color = GUI.color;
            if (type == PowerType.Weakness) GUI.color = new Color(0.6f, 0f, 0f);
            GUI.DrawTexture(inRect, ColonistBar.BGTex);
            GUI.color = color;
            if (EditMode && !filled && Widgets.ButtonInvisible(inRect, false))
                Find.WindowStack.Add(new FloatMenu(DefDatabase<PowerDef>.AllDefs.Where(power => power.powerType == type)
                    .Select(power => new FloatMenuOption(power.LabelCap, () => SelPowerTracker.AddPower(power))).ToList()));
        }

        public void DoPowerIcon(Rect inRect, PowerDef power)
        {
            GUI.DrawTexture(inRect, power.Icon);
            TooltipHandler.TipRegion(inRect, new TipSignal($"{power.LabelCap}\n\n{power.description}\n{power.Worker.EffectString()}"));
            Widgets.DrawHighlightIfMouseover(inRect);
            if (EditMode && Widgets.ButtonInvisible(inRect, false))
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption> {new FloatMenuOption("Remove", () => SelPowerTracker.RemovePower(power))}));
        }
    }
}