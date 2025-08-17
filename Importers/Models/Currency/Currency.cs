using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Importers.Models;

[Table("Currencies")]
public class Currency : IEquatable<Currency>
{
    [JsonPropertyName("code")]
    public required string CurrencyCode { get; init; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; }

    [JsonPropertyName("decimal_digits")]
    public required byte MinorUnitFractions { get; init; } // see : https://en.wikipedia.org/wiki/ISO_4217#Minor_unit_fractions

    public static Currency? GetCurrency(string codeOrSymbol)
    {
        return GetCurrency(codeOrSymbol, GetCurrencies());
    }

    public static Currency? GetCurrency(string codeOrSymbol, IEnumerable<Currency> currencies)
    {
        return currencies.FirstOrDefault(c =>
            codeOrSymbol == c.CurrencyCode || codeOrSymbol == c.Symbol
        );
    }

    public static IList<Currency> GetCurrencies()
    {
        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string jsonPath = Path.Combine(assemblyPath, "SeedData", "Common-Currency.seed.json"); //from gist.githubusercontent.com/ksafranski/2973986/raw/5fda5e87189b066e11c1bf80bbfbecb556cf2cc1/Common-Currency.json
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
        return currencies;
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
