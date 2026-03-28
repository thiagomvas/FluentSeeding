using System.Text.Json.Serialization;

namespace FluentSeeding.Faker.Locales;

internal sealed class WordLocaleData
{
    [JsonPropertyName("adjectives")]
    public string[] Adjectives { get; init; } = null!;

    [JsonPropertyName("nouns")]
    public string[] Nouns { get; init; } = null!;

    [JsonPropertyName("verbs")]
    public string[] Verbs { get; init; } = null!;

    [JsonPropertyName("adverbs")]
    public string[] Adverbs { get; init; } = null!;

    [JsonPropertyName("colors")]
    public string[] Colors { get; init; } = null!;

    [JsonPropertyName("animals")]
    public string[] Animals { get; init; } = null!;
}
