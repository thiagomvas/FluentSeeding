using System.Text.Json.Serialization;

namespace FluentSeeding.Faker.Locales;

internal sealed record LocaleData
{
    [JsonPropertyName("internet")]
    public InternetLocaleData Internet { get; init; } = null!;

    [JsonPropertyName("person")]
    public PersonLocaleData Person { get; init; } = null!;

    [JsonPropertyName("address")]
    public AddressLocaleData Address { get; init; } = null!;

    [JsonPropertyName("words")]
    public WordLocaleData Words { get; init; } = null!;
    
    [JsonPropertyName("finance")]
    public FinanceLocaleData Finance { get; init; } = null!;
}
