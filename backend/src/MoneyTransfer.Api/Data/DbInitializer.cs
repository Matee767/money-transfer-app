using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Api.Models;

namespace MoneyTransfer.Api.Data;

public static class DbInitializer
{
    /// <summary>
    /// Creates the schema and seeds demo accounts. Retries while SQL Server
    /// is still starting up (relevant when the whole stack boots via
    /// docker compose and the database container is not ready yet).
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DbInitializer));

        const int maxAttempts = 30;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(ct);
                break;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    "Database not ready (attempt {Attempt}/{Max}): {Message}. Retrying in 2s...",
                    attempt, maxAttempts, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
        }

        if (!await db.Accounts.AnyAsync(ct))
        {
            db.Accounts.AddRange(
                new Account { Owner = "Giorgi Beridze", Balance = 1000.00m },
                new Account { Owner = "Nino Kapanadze", Balance = 2500.50m },
                new Account { Owner = "Luka Tsereteli", Balance = 300.00m },
                new Account { Owner = "Mariam Gelashvili", Balance = 0.00m });

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded demo accounts.");
        }
    }
}
