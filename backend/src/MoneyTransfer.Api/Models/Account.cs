namespace MoneyTransfer.Api.Models;

public class Account
{
    public int Id { get; set; }

    public required string Owner { get; set; }

    public decimal Balance { get; set; }

    /// <summary>
    /// Optimistic concurrency token. Prevents lost updates when two
    /// transfers touch the same account at the same time.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
