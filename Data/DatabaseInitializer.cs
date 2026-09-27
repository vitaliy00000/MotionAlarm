using Microsoft.EntityFrameworkCore;

namespace MotionAlarm.Data;

public sealed class DatabaseInitializer
{
    private readonly IDbContextFactory<AlarmDbContext> _factory;

    public DatabaseInitializer(IDbContextFactory<AlarmDbContext> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            await db.Database.EnsureCreatedAsync(ct);
        }
        catch (Exception)
        {
            throw;
        }
    }
}