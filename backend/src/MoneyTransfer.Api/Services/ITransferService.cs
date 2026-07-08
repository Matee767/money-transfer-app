namespace MoneyTransfer.Api.Services;

public interface ITransferService
{
    /// <summary>
    /// Moves <paramref name="amount"/> from one account to another.
    /// The debit, credit and transaction record are persisted atomically;
    /// the operation fails without side effects if funds are insufficient.
    /// </summary>
    Task<TransferResult> TransferAsync(
        int fromAccountId, int toAccountId, decimal amount, CancellationToken ct = default);
}
