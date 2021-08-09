using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VFEAncients
{
    [StaticConstructorOnStartup]
    public class Command_AbilityCooldown : Command_Ability
    {
        public static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public Command_AbilityCooldown(Pawn pawn, VFECore.Abilities.Ability ability) : base(pawn, ability)
        {
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUIInt(butRect, parms);
            if (ability.cooldown > Find.TickManager.TicksGame)
                GUI.DrawTexture(butRect.RightPartPixels(butRect.width * (ability.cooldown - Find.TickManager.TicksGame) / ability.GetCooldownForPawn()), CooldownTex);
            return result;
        }
    }
}