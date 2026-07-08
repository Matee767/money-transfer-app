using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Api.Contracts;

public record AccountDto(int Id, string Owner, decimal Balance);

public record TransactionDto(
    long Id,
    int FromAccountId,
    string FromOwner,
    int ToAccountId,
    string ToOwner,
    decimal Amount,
    DateTime CreatedAtUtc);

public record TransferRequest(
    int FromAccountId,
    int ToAccountId,
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    decimal Amount);

/// <summary>
/// Returned on a successful transfer: the recorded transaction plus the
/// resulting balances of both accounts.
/// </summary>
public record TransferResponse(
    long TransactionId,
    int FromAccountId,
    decimal FromAccountBalance,
    int ToAccountId,
    decimal ToAccountBalance,
    decimal Amount,
    DateTime CreatedAtUtc);
