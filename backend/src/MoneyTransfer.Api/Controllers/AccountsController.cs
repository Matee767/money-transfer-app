using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Contracts;
using MoneyTransfer.Api.Data;

namespace MoneyTransfer.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAccounts(CancellationToken ct)
    {
        var accounts = await db.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new AccountDto(a.Id, a.Owner, a.Balance))
            .ToListAsync(ct);

        return Ok(accounts);
    }
}
