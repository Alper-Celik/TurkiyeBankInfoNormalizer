using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Importers.Models;

public class Country
{
    [JsonPropertyName("alpha3")]
    public required string Alpha3Code { get; set; }

    [JsonPropertyName("alpha2")]
    public required string Alpha2Code { get; set; }

    [JsonPropertyName("id")]
    public required short NumericCode { get; set; }

    [JsonPropertyName("name")]
    public required string EnglishName { get; set; }

    public static Country? GetCountry(string codeOrName)
    {
        Country? result = null;

        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string jsonPath = Path.Combine(assemblyPath, "SeedData", "countries.seed.json"); //from https://github.com/stefangabos/world_countries/blob/3480efd5b52aee45ebc22afa224cc05b70c500df/data/countries/en/countries.json
        List<Country> countries =
            JsonSerializer.Deserialize<List<Country>>(File.ReadAllText(jsonPath))
            ?? throw new UnreachableException("json not found");

        foreach (Country country in countries)
        {
            int i = 0;
            bool found = false;
            do
            {
                if (codeOrName == country.Alpha2Code || codeOrName == country.Alpha3Code)
                {
                    found = true;
                }
                country.Alpha2Code = country.Alpha2Code.ToUpper(CultureInfo.InvariantCulture);
                country.Alpha3Code = country.Alpha3Code.ToUpper(CultureInfo.InvariantCulture);
                i++;
            } while (i != 2);

            if (found)
            {
                return country;
            }
        }

        return result;
    }
}
