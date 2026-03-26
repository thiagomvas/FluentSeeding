namespace FluentSeeding.Faker.Extensions;

public static class BrazilianSeedRuleExtensions
{
    // Value = ch - '0' (ASCII - 48): digits → 0–9, letters A–Z → 17–42.
    private static readonly Dictionary<char, int> cnpjWeights = new()
    {
        { '0',  0 }, { '1',  1 }, { '2',  2 }, { '3',  3 }, { '4',  4 },
        { '5',  5 }, { '6',  6 }, { '7',  7 }, { '8',  8 }, { '9',  9 },
        { 'A', 17 }, { 'B', 18 }, { 'C', 19 }, { 'D', 20 }, { 'E', 21 },
        { 'F', 22 }, { 'G', 23 }, { 'H', 24 }, { 'I', 25 }, { 'J', 26 },
        { 'K', 27 }, { 'L', 28 }, { 'M', 29 }, { 'N', 30 }, { 'O', 31 },
        { 'P', 32 }, { 'Q', 33 }, { 'R', 34 }, { 'S', 35 }, { 'T', 36 },
        { 'U', 37 }, { 'V', 38 }, { 'W', 39 }, { 'X', 40 }, { 'Y', 41 },
        { 'Z', 42 },
    };
    private const long CnpjSpace = 4_738_381_338_321_616_896L;

    public static SeedBuilder<T> UseCpf<T>(this SeedRule<T, string> rule, bool formatted = false) where T : class
    {
        return rule.UseFactory(() => GenerateCpf(formatted));
    }

    public static SeedBuilder<T> UseCnpj<T>(this SeedRule<T, string> rule, bool formatted = false) where T : class
    {
        return rule.UseFactory(() => GenerateCnpj(formatted));
    }

    private static string GenerateCpf(bool formatted = false)
    {
        var n = Random.Shared.Next(0, 1_000_000_000);

        var d0 = n / 100_000_000 % 10;
        var d1 = n / 10_000_000 % 10;
        var d2 = n / 1_000_000 % 10;
        var d3 = n / 100_000 % 10;
        var d4 = n / 10_000 % 10;
        var d5 = n / 1_000 % 10;
        var d6 = n / 100 % 10;
        var d7 = n / 10 % 10;
        var d8 = n % 10;

        var sum1 = d0 * 10 + d1 * 9 + d2 * 8 + d3 * 7 + d4 * 6 + d5 * 5 + d6 * 4 + d7 * 3 + d8 * 2;
        var r1 = sum1 % 11;
        var c1 = r1 < 2 ? 0 : 11 - r1;

        var sum2 = d0 * 11 + d1 * 10 + d2 * 9 + d3 * 8 + d4 * 7 + d5 * 6 + d6 * 5 + d7 * 4 + d8 * 3 + c1 * 2;
        var r2 = sum2 % 11;
        var c2 = r2 < 2 ? 0 : 11 - r2;

        if (formatted)
            return $"{d0}{d1}{d2}.{d3}{d4}{d5}.{d6}{d7}{d8}-{c1}{c2}";
        return $"{d0}{d1}{d2}{d3}{d4}{d5}{d6}{d7}{d8}{c1}{c2}";
    }


    private static string GenerateCnpj(bool formatted)
    {
        var r = Random.Shared.NextInt64(0, CnpjSpace);

        var v0  = (int)(r % 36); r /= 36;
        var v1  = (int)(r % 36); r /= 36;
        var v2  = (int)(r % 36); r /= 36;
        var v3  = (int)(r % 36); r /= 36;
        var v4  = (int)(r % 36); r /= 36;
        var v5  = (int)(r % 36); r /= 36;
        var v6  = (int)(r % 36); r /= 36;
        var v7  = (int)(r % 36); r /= 36;
        var v8  = (int)(r % 36); r /= 36;
        var v9  = (int)(r % 36); r /= 36;
        var v10 = (int)(r % 36); r /= 36;
        var v11 = (int)r;

        static int W(int v) => v < 10 ? v : v + 7;
        static char C(int v) => v < 10 ? (char)('0' + v) : (char)('A' + v - 10);

        var sum1 = W(v0)*5 + W(v1)*4 + W(v2)*3 + W(v3)*2 + W(v4)*9 + W(v5)*8
                 + W(v6)*7 + W(v7)*6 + W(v8)*5 + W(v9)*4 + W(v10)*3 + W(v11)*2;
        var rem1 = sum1 % 11;
        var dv1  = rem1 < 2 ? 0 : 11 - rem1;

        var sum2 = W(v0)*6 + W(v1)*5 + W(v2)*4 + W(v3)*3 + W(v4)*2 + W(v5)*9
                 + W(v6)*8 + W(v7)*7 + W(v8)*6 + W(v9)*5 + W(v10)*4 + W(v11)*3 + dv1*2;
        var rem2 = sum2 % 11;
        var dv2  = rem2 < 2 ? 0 : 11 - rem2;

        var c0  = C(v0);  var c1  = C(v1);  var c2  = C(v2);  var c3  = C(v3);
        var c4  = C(v4);  var c5  = C(v5);  var c6  = C(v6);  var c7  = C(v7);
        var c8  = C(v8);  var c9  = C(v9);  var c10 = C(v10); var c11 = C(v11);

        if (formatted)
            return $"{c0}{c1}.{c2}{c3}{c4}.{c5}{c6}{c7}/{c8}{c9}{c10}{c11}-{dv1}{dv2}";
        return $"{c0}{c1}{c2}{c3}{c4}{c5}{c6}{c7}{c8}{c9}{c10}{c11}{dv1}{dv2}";
    }
}