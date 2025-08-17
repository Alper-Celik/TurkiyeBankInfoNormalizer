using System.ComponentModel.DataAnnotations.Schema;

namespace Importers.Models;

[Table("CardTransactions")]
public class CardTransaction
{
    public int Id { get; set; }
    public required DateOnly TransactionDate { get; set; }
    public TimeOnly? TransactionTime { get; set; }
    public long AmountInMinorUnit { get; set; }
    public required string Comment { get; set; }

    [ForeignKey(nameof(Currency))]
    public required string CurrencyCode { get; set; }
    public required Currency Currency { get; set; }

    [ForeignKey(nameof(Country))]
    public string? CountryAlpha3Code { get; set; }
    public Country? Country { get; set; }

    [ForeignKey(nameof(Card))]
    public int CardId { get; set; }
    public required Card Card { get; set; }
    // ...
}
