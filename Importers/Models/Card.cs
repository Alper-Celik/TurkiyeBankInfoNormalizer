namespace Importers.Models;

public class Card
{
    public string? AvailableCardNumberPart { get; set; }
    public required string Name { get; set; }
    public CardTypes? CardType { get; set; }
    public string IssuedBank { get; set; } = string.Empty;
}

public enum CardTypes
{
    CreditCard,
    DebitCard,
    PrePaidCard,
}
