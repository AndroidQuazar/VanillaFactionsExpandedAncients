using Verse;

namespace VFEAncients
{
    public class RecipeWorker_Mend : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            var def = ingredient.def;
            if (recipe.HasModExtension<RecipeExtension_Mend>() && ingredient.stackCount == 1 && (def.IsWeapon || def.IsApparel) && def.useHitPoints)
                ingredient.DeSpawn();
            else
                base.ConsumeIngredient(ingredient, recipe, map);
        }
    }
}