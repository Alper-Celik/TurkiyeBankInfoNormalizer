using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Models.Currency;

[Table("Currencies")]
public sealed class Currency : IEquatable<Currency>
{
    [JsonPropertyName("code")]
    public required string CurrencyCode { get; init; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; }

    [JsonPropertyName("decimal_digits")]
    public required byte MinorUnitFractions { get; init; } // see : https://en.wikipedia.org/wiki/ISO_4217#Minor_unit_fractions

    private static List<Currency>? s_currencies;
    private static readonly Lock CurrenciesLock = new();

    public static Currency? GetCurrency(string codeOrSymbol)
    {
        return GetCurrencies()
            .FirstOrDefault(c => codeOrSymbol == c.CurrencyCode || codeOrSymbol == c.Symbol);
    }

    public static IList<Currency> GetCurrencies()
    {
        if (s_currencies is null)
        {
            lock (CurrenciesLock)
            {
                string assemblyPath = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                )!;
                string jsonPath = Path.Combine(
                    assemblyPath,
                    "SeedData",
                    "Common-Currency.seed.json"
                ); //from gist.githubusercontent.com/ksafranski/2973986/raw/5fda5e87189b066e11c1bf80bbfbecb556cf2cc1/Common-Currency.json
                JsonNode? currencies_objects = JsonSerializer.Deserialize<JsonNode>(
                    File.ReadAllText(jsonPath)
                );

                List<Currency> currencies = [];

                foreach (
                    KeyValuePair<string, JsonNode?> currency_obj in currencies_objects?.AsObject()
                        ?? throw new UnreachableException("can't parse currency seed json")
                )
                {
                    currencies.Add(
                        JsonSerializer.Deserialize<Currency>(currency_obj.Value)
                            ?? throw new UnreachableException("cannot parse currency seed json")
                    );
                }

                s_currencies = currencies;
            }
        }
        return s_currencies;
    }

    public bool Equals(Currency? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(CurrencyCode, other.CurrencyCode, StringComparison.InvariantCulture)
            && string.Equals(Symbol, other.Symbol, StringComparison.InvariantCulture)
            && string.Equals(Name, other.Name, StringComparison.InvariantCulture)
            && MinorUnitFractions == other.MinorUnitFractions;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Currency)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(CurrencyCode, StringComparer.InvariantCulture);
        hashCode.Add(Symbol, StringComparer.InvariantCulture);
        hashCode.Add(Name, StringComparer.InvariantCulture);
        hashCode.Add(MinorUnitFractions);
        return hashCode.ToHashCode();
    }
}
