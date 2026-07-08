using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<TransferTransaction> Transactions => Set<TransferTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(account =>
        {
            account.Property(a => a.Owner).HasMaxLength(200);
            account.Property(a => a.Balance).HasPrecision(18, 2);
            account.Property(a => a.RowVersion).IsRowVersion();

            // Hard invariant enforced by the database itself: no balance may
            // ever go negative, regardless of application-level bugs or races.
            account.ToTable(t => t.HasCheckConstraint(
                "CK_Accounts_Balance_NonNegative", "[Balance] >= 0"));
        });

        modelBuilder.Entity<TransferTransaction>(tx =>
        {
            tx.Property(t => t.Amount).HasPrecision(18, 2);

            tx.HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            tx.HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            tx.HasIndex(t => t.CreatedAtUtc);
        });
    }
}
