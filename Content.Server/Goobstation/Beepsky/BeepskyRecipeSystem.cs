public override void Initialize()
{
    base.Initialize();

    // Register existing recipes
    RegisterRecipe("construct_some_other_item");
    RegisterRecipe("construct_another_item");

    // Register the Beepsky Chassis construction recipe
    var beepskyRecipe = LoadRecipeFromYaml("/Prototypes/Crafting/construct_beepsky_chassis.yaml");
    if (beepskyRecipe != null)
    {
        RegisterRecipe(beepskyRecipe);
    }
    else
    {
        Logger.WarningS("crafting", "Failed to register Beepsky Chassis recipe");
    }
}

private void RegisterRecipe(string recipeId)
{
    if (_prototypeManager.TryIndex(recipeId, out CraftingRecipePrototype? recipe))
    {
        // Assuming your system has a method to register these recipes
        _prototypeManager.RegisterPrototype(recipe);
    }
    else
    {
        Logger.WarningS("crafting", $"Recipe with ID {recipeId} not found");
    }
}
