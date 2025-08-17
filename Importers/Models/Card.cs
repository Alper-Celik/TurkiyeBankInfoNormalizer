using System.ComponentModel.DataAnnotations.Schema;

namespace Importers.Models;

public class Card
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public CardTypes CardType { get; set; }

    public int BankId { get; set; }

    public string IssuedBank { get; set; } = string.Empty;
}

public enum CardTypes
{
    CreditCard,
    DebitCard,
    PrePaidCard,
}
