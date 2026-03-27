using System.Text.Json.Serialization;

namespace FluentSeeding.Faker.Locales;

internal sealed record LocaleData
{
    [JsonPropertyName("internet")]
    public InternetLocaleData Internet { get; init; } = null!;

    [JsonPropertyName("person")]
    public PersonLocaleData Person { get; init; } = null!;
}
