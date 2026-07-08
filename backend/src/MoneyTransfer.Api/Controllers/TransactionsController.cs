using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Contracts;
using MoneyTransfer.Api.Data;

namespace MoneyTransfer.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetTransactions(CancellationToken ct)
    {
        var transactions = await db.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Take(100)
            .Select(t => new TransactionDto(
                t.Id,
                t.FromAccountId,
                t.FromAccount!.Owner,
                t.ToAccountId,
                t.ToAccount!.Owner,
                t.Amount,
                t.CreatedAtUtc))
            .ToListAsync(ct);

        return Ok(transactions);
    }
}
