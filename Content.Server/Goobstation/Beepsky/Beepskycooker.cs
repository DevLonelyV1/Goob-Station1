using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public Recipe LoadRecipeFromYaml(string recipeId)
{
    var filePath = $"Recipes/{recipeId}.yaml";
    if (!File.Exists(filePath))
        return null;

    var yamlText = File.ReadAllText(filePath);
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    var recipe = deserializer.Deserialize<Recipe>(yamlText);
    return recipe;
}
