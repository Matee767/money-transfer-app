using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MoneyTransfer.Api.Data;
using MoneyTransfer.Api.Models;
using MoneyTransfer.Api.Services;
using Xunit;

namespace MoneyTransfer.Api.Tests;

public class TransferServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(AppDbContext Db, Account From, Account To)> SeedAsync(
        decimal fromBalance = 100m, decimal toBalance = 50m)
    {
        var db = CreateContext();
        var from = new Account { Owner = "Sender", Balance = fromBalance };
        var to = new Account { Owner = "Receiver", Balance = toBalance };
        db.Accounts.AddRange(from, to);
        await db.SaveChangesAsync();
        return (db, from, to);
    }

    private static TransferService CreateService(AppDbContext db) =>
        new(db, NullLogger<TransferService>.Instance);

    [Fact]
    public async Task Transfer_WithSufficientFunds_MovesMoneyAndRecordsTransaction()
    {
        var (db, from, to) = await SeedAsync(fromBalance: 100m, toBalance: 50m);
        var service = CreateService(db);

        var result = await service.TransferAsync(from.Id, to.Id, 30m);

        Assert.True(result.Succeeded);
        Assert.Equal(70m, result.FromAccount!.Balance);
        Assert.Equal(80m, result.ToAccount!.Balance);

        var stored = await db.Transactions.SingleAsync();
        Assert.Equal(from.Id, stored.FromAccountId);
        Assert.Equal(to.Id, stored.ToAccountId);
        Assert.Equal(30m, stored.Amount);
    }

    [Fact]
    public async Task Transfer_OfEntireBalance_SucceedsAndLeavesZero()
    {
        var (db, from, to) = await SeedAsync(fromBalance: 100m);
        var service = CreateService(db);

        var result = await service.TransferAsync(from.Id, to.Id, 100m);

        Assert.True(result.Succeeded);
        Assert.Equal(0m, result.FromAccount!.Balance);
    }

    [Fact]
    public async Task Transfer_WithInsufficientFunds_FailsWithoutSideEffects()
    {
        var (db, from, to) = await SeedAsync(fromBalance: 20m, toBalance: 50m);
        var service = CreateService(db);

        var result = await service.TransferAsync(from.Id, to.Id, 20.01m);

        Assert.False(result.Succeeded);
        Assert.Equal(TransferError.InsufficientFunds, result.Error);

        db.ChangeTracker.Clear();
        Assert.Equal(20m, (await db.Accounts.SingleAsync(a => a.Id == from.Id)).Balance);
        Assert.Equal(50m, (await db.Accounts.SingleAsync(a => a.Id == to.Id)).Balance);
        Assert.Empty(await db.Transactions.ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Transfer_WithNonPositiveAmount_Fails(decimal amount)
    {
        var (db, from, to) = await SeedAsync();
        var service = CreateService(db);

        var result = await service.TransferAsync(from.Id, to.Id, amount);

        Assert.Equal(TransferError.InvalidAmount, result.Error);
        Assert.Empty(await db.Transactions.ToListAsync());
    }

    [Fact]
    public async Task Transfer_ToSameAccount_Fails()
    {
        var (db, from, _) = await SeedAsync();
        var service = CreateService(db);

        var result = await service.TransferAsync(from.Id, from.Id, 10m);

        Assert.Equal(TransferError.SameAccount, result.Error);
    }

    [Fact]
    public async Task Transfer_FromMissingAccount_Fails()
    {
        var (db, _, to) = await SeedAsync();
        var service = CreateService(db);

        var result = await service.TransferAsync(9999, to.Id, 10m);

        Assert.Equal(TransferError.SourceAccountNotFound, result.Error);
    }

    [Fact]
    public async Task Transfer_ToMissingAccount_Fails()
    {
        var (db, from, _) = await SeedAsync();
        var service = CreateService(db);

        var result = await service.TransferAsync(from.Id, 9999, 10m);

        Assert.Equal(TransferError.DestinationAccountNotFound, result.Error);
    }
}
