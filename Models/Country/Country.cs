// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Models.Country;

public sealed class Country : IEquatable<Country>
{
    [JsonPropertyName("alpha3")]
    public required string Alpha3Code { get; init; }

    [JsonPropertyName("alpha2")]
    public required string Alpha2Code { get; init; }

    [JsonPropertyName("id")]
    public required short NumericCode { get; init; }

    [JsonPropertyName("name")]
    public required string EnglishName { get; init; }

    private static List<Country>? s_countries;
    private static readonly Lock CountriesLock = new();

    public static IList<Country> GetCountries()
    {
        if (s_countries is null)
        {
            lock (CountriesLock)
            {
                string assemblyPath = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                )!;
                string jsonPath = Path.Combine(assemblyPath, "SeedData", "countries.seed.json"); //from https://github.com/stefangabos/world_countries/blob/3480efd5b52aee45ebc22afa224cc05b70c500df/data/countries/en/countries.json
                List<Country> countries =
                    JsonSerializer.Deserialize<List<Country>>(File.ReadAllText(jsonPath))
                    ?? throw new UnreachableException("json not found");
                s_countries = countries;
            }
        }
        return s_countries;
    }

    public static Country? GetCountry(string codeOrName)
    {
        Country? result = null;

        foreach (Country country in GetCountries())
        {
            int i = 0;
            bool found = false;
            string Alpha2Code = country.Alpha2Code;
            string Alpha3Code = country.Alpha3Code;
            do
            {
                if (codeOrName == Alpha2Code || codeOrName == Alpha3Code)
                {
                    found = true;
                }
                Alpha2Code = country.Alpha2Code.ToUpper(CultureInfo.InvariantCulture);
                Alpha3Code = country.Alpha3Code.ToUpper(CultureInfo.InvariantCulture);
                i++;
            } while (i != 2);

            if (found)
            {
                return country;
            }
        }

        return result;
    }

    public bool Equals(Country? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Alpha3Code, other.Alpha3Code, StringComparison.InvariantCulture)
            && string.Equals(Alpha2Code, other.Alpha2Code, StringComparison.InvariantCulture)
            && NumericCode == other.NumericCode
            && string.Equals(EnglishName, other.EnglishName, StringComparison.InvariantCulture);
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

        return Equals((Country)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Alpha3Code, StringComparer.InvariantCulture);
        hashCode.Add(Alpha2Code, StringComparer.InvariantCulture);
        hashCode.Add(NumericCode);
        hashCode.Add(EnglishName, StringComparer.InvariantCulture);
        return hashCode.ToHashCode();
    }
}
