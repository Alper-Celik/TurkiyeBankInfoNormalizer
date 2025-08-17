using System.ComponentModel.DataAnnotations.Schema;

namespace Importers.Models;

public class CardTransaction
{
    public int Id { get; set; }
    public required DateOnly TransactionDate { get; set; }
    public TimeOnly? TransactionTime { get; set; }
    public long AmountInMinorUnit { get; set; }
    public required string Comment { get; set; }
    public required Currency Currency { get; set; }
    public Country? Country { get; set; }
    public required Card Card { get; set; }
    public string? Category { get; set; }
}
