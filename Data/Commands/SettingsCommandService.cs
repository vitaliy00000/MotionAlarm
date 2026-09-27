using Microsoft.EntityFrameworkCore;
using MotionAlarm.Data.Entities;
using MotionAlarm.Models;
using System.Text.Json;

namespace MotionAlarm.Data.Commands;

public class SettingsCommandService
{
    private const int SettingsRowId = 1;
    private readonly IDbContextFactory<AlarmDbContext> _factory;

    public SettingsCommandService(IDbContextFactory<AlarmDbContext> factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(AlarmSettings settings, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.Settings.FirstOrDefaultAsync(x => x.Id == SettingsRowId, ct);

        if (row is null)
        {
            row = new SettingsEntity { Id = SettingsRowId };
            db.Settings.Add(row);
        }

        row.Json = JsonSerializer.Serialize(settings);
        await db.SaveChangesAsync(ct);
    }
}
