using Microsoft.AspNetCore.Mvc;
using MoneyTransfer.Api.Contracts;
using MoneyTransfer.Api.Services;

namespace MoneyTransfer.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public class TransfersController(ITransferService transferService) : ControllerBase
{
    /// <summary>
    /// Transfers money from one account to another (simulation: only the
    /// balances stored in the database change).
    /// </summary>
    [HttpPost]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Transfer(TransferRequest request, CancellationToken ct)
    {
        var result = await transferService.TransferAsync(
            request.FromAccountId, request.ToAccountId, request.Amount, ct);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                TransferError.InvalidAmount => Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid amount",
                    detail: "The transfer amount must be greater than zero."),

                TransferError.SameAccount => Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Same account",
                    detail: "The source and destination accounts must be different."),

                TransferError.SourceAccountNotFound => Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Source account not found",
                    detail: $"Account {request.FromAccountId} does not exist."),

                TransferError.DestinationAccountNotFound => Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Destination account not found",
                    detail: $"Account {request.ToAccountId} does not exist."),

                TransferError.InsufficientFunds => Problem(
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Insufficient funds",
                    detail: $"Account {request.FromAccountId} does not have enough balance for this transfer."),

                TransferError.ConcurrencyConflict => Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Concurrent update conflict",
                    detail: "The transfer could not be completed due to concurrent activity. Please try again."),

                _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
            };
        }

        var tx = result.Transaction!;
        var response = new TransferResponse(
            tx.Id,
            result.FromAccount!.Id,
            result.FromAccount.Balance,
            result.ToAccount!.Id,
            result.ToAccount.Balance,
            tx.Amount,
            tx.CreatedAtUtc);

        return CreatedAtAction(nameof(Transfer), new { id = tx.Id }, response);
    }
}
