namespace FluentSeeding.Faker.Extensions;

public static class FinanceSeedRuleExtensions
{
    /// <summary>
    /// Picks a random ISO 4217 currency code drawn from the builder's locale
    /// (set via <c>WithLocale</c>, falling back to <see cref="FluentFaker.DefaultLocale"/>).
    /// </summary>
    public static SeedBuilder<T> UseCurrencyCode<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Finance.CurrencyCodes);
    }

    /// <summary>
    /// Picks a random credit card type (e.g. <c>Visa</c>, <c>Mastercard</c>) drawn from the builder's locale
    /// (set via <c>WithLocale</c>, falling back to <see cref="FluentFaker.DefaultLocale"/>).
    /// </summary>
    public static SeedBuilder<T> UseCreditCardType<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Finance.CreditCardType);
    }
    
    /// <summary>
    /// Picks a random bank name drawn from the builder's locale (set via <c>WithLocale</c>, falling back to <see cref="FluentFaker.DefaultLocale"/>).
    /// </summary> 
    public static SeedBuilder<T> UseBank<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Finance.Banks);
    }
}