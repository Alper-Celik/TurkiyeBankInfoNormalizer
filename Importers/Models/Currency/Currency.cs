using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Importers.Models;

[Table("Currencies")]
public class Currency
{
    [JsonPropertyName("code")]
    public required string CurrencyCode { get; set; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("decimal_digits")]
    public required byte MinorUnitFractions { get; set; } // see : https://en.wikipedia.org/wiki/ISO_4217#Minor_unit_fractions

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
}
