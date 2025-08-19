using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Models;

public class CardTransaction
{
    [Format("o")]
    public required DateOnly TransactionDate { get; set; }

    [Format("o")]
    public TimeOnly? TransactionTime { get; set; }

    public long AmountInMinorUnit { get; set; }

    public required string Comment { get; set; }

    [HeaderPrefix("Currency.")]
    public required Currency.Currency Currency { get; set; }

    [HeaderPrefix("Country.")]
    public Country.Country? Country { get; set; }

    [HeaderPrefix("Card.")]
    public required Card Card { get; set; }

    public string? Category { get; set; }
}
