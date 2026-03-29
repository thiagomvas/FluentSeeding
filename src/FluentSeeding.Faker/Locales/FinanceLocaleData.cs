using System.Text.Json.Serialization;

namespace FluentSeeding.Faker.Locales;

public class FinanceLocaleData
{
    public readonly string[] CreditCardType = ["Visa", "MasterCard", "American Express", "Discover", "Diners Club", "JCB"];
    public readonly string[] CurrencyCodes = ["USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD", "MXN", "SGD", "HKD", "NOK", "KRW", "TRY", "RUB", "INR", "BRL", "ZAR",
        "DKK", "PLN", "TWD", "THB", "MYR", "IDR", "CZK", "HUF", "ILS", "CLP", "PHP", "AED", "COP", "SAR", "RON"];

    [JsonPropertyName("bank")] 
    public string[] Banks { get; init; } = null!;
}