using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Services;

public enum TransferError
{
    None = 0,
    SameAccount,
    InvalidAmount,
    SourceAccountNotFound,
    DestinationAccountNotFound,
    InsufficientFunds,
    ConcurrencyConflict,
}

public record TransferResult(
    TransferError Error,
    TransferTransaction? Transaction = null,
    Account? FromAccount = null,
    Account? ToAccount = null)
{
    public bool Succeeded => Error == TransferError.None;

    public static TransferResult Success(TransferTransaction tx, Account from, Account to) =>
        new(TransferError.None, tx, from, to);

    public static TransferResult Failure(TransferError error) => new(error);
}
