using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Services;

public class TransferService(AppDbContext db, ILogger<TransferService> logger) : ITransferService
{
    /// <summary>
    /// How many times a transfer is retried when it loses an optimistic
    /// concurrency race with another transfer touching the same account.
    /// </summary>
    private const int MaxAttempts = 3;

    public async Task<TransferResult> TransferAsync(
        int fromAccountId, int toAccountId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0)
        {
            return TransferResult.Failure(TransferError.InvalidAmount);
        }

        if (fromAccountId == toAccountId)
        {
            return TransferResult.Failure(TransferError.SameAccount);
        }

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            db.ChangeTracker.Clear();

            var from = await db.Accounts.SingleOrDefaultAsync(a => a.Id == fromAccountId, ct);
            if (from is null)
            {
                return TransferResult.Failure(TransferError.SourceAccountNotFound);
            }

            var to = await db.Accounts.SingleOrDefaultAsync(a => a.Id == toAccountId, ct);
            if (to is null)
            {
                return TransferResult.Failure(TransferError.DestinationAccountNotFound);
            }

            if (from.Balance < amount)
            {
                return TransferResult.Failure(TransferError.InsufficientFunds);
            }

            from.Balance -= amount;
            to.Balance += amount;

            var transaction = new TransferTransaction
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = amount,
                CreatedAtUtc = DateTime.UtcNow,
            };
            db.Transactions.Add(transaction);

            try
            {
                // Both balance changes and the transaction record are written
                // in a single SaveChanges, i.e. one database transaction.
                await db.SaveChangesAsync(ct);
                return TransferResult.Success(transaction, from, to);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another transfer modified one of the accounts between our
                // read and write (RowVersion mismatch). Reload and re-validate.
                logger.LogInformation(
                    "Concurrency conflict transferring {Amount} from {From} to {To} (attempt {Attempt}/{Max}).",
                    amount, fromAccountId, toAccountId, attempt, MaxAttempts);
            }
            catch (DbUpdateException)
            {
                // Defense in depth: the CK_Accounts_Balance_NonNegative check
                // constraint rejected the write. Treat as insufficient funds.
                return TransferResult.Failure(TransferError.InsufficientFunds);
            }
        }

        return TransferResult.Failure(TransferError.ConcurrencyConflict);
    }
}
