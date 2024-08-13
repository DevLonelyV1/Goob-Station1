// This should be the top of the file
using System;
using Content.Server.Silicons.Borgs;

namespace Content.Server.Goobstation.Beepsky
{
    public class BeepskyRecipeSystem
    {
        // Place your public/private fields, methods, and properties here
        
        public void Initialize()
        {
            base.Initialize();

            // Register existing recipes
            // ...

            // Register the Beepsky Chassis construction recipe
            var beepskyRecipe = LoadRecipeFromYaml("construct_beepsky_chassis");
            if (beepskyRecipe != null)
            {
                RegisterRecipe(beepskyRecipe);
            }
            else
            {
                Logger.WarningS("crafting", "Failed to register Beepsky Chassis recipe");
            }
        }
    }
}
