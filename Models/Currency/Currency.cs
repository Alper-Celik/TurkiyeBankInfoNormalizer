// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Models.Currency;

public sealed record Currency
{
    [JsonIgnore]
    public string CurrencyCode { get; init; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; }

    [JsonPropertyName("ISOdigits")]
    public byte MinorUnitFractions { get; init; } // see : https://en.wikipedia.org/wiki/ISO_4217#Minor_unit_fractions

    private static List<Currency>? _sCurrencies;
    private static readonly Lock CurrenciesLock = new();

    public static Currency? GetCurrency(string codeOrSymbol)
    {
        return GetCurrencies()
            .FirstOrDefault(c => codeOrSymbol == c.CurrencyCode || codeOrSymbol == c.Symbol);
    }

    public static IList<Currency> GetCurrencies()
    {
        if (_sCurrencies is null)
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
                ); //from https://github.com/ourworldincode/currency/blob/a083d35f0fbe595146ced760b3e9ed1f0d3ecf03/currencies.json
                // TODO: finish transition to new repo
                JsonNode? currenciesObjects = JsonSerializer.Deserialize<JsonNode>(
                    File.ReadAllText(jsonPath)
                );

                List<Currency> currencies = [];

                foreach (
                    KeyValuePair<string, JsonNode?> currencyObj in currenciesObjects?.AsObject()
                        ?? throw new UnreachableException("can't parse currency seed json")
                )
                {
                    currencies.Add(
                        (
                            currencyObj.Value.Deserialize<Currency>()
                            ?? throw new UnreachableException("cannot parse currency seed json")
                        ) with
                        {
                            CurrencyCode = currencyObj.Key,
                        }
                    );
                }

                _sCurrencies = currencies;
            }
        }
        return _sCurrencies;
    }
}
