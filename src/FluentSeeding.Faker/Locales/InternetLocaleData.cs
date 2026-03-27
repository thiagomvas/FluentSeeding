using System.Text.Json.Serialization;

namespace FluentSeeding.Faker.Locales;

internal sealed class InternetLocaleData
{
    [JsonPropertyName("email_suffix")]
    public string[] EmailSuffix { get; init; } = null!;
}
