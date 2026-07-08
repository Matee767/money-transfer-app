namespace MoneyTransfer.Api.Models;

/// <summary>
/// A successfully completed transfer between two accounts.
/// </summary>
public class TransferTransaction
{
    public long Id { get; set; }

    public int FromAccountId { get; set; }
    public Account? FromAccount { get; set; }

    public int ToAccountId { get; set; }
    public Account? ToAccount { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
