using Microsoft.EntityFrameworkCore;
using MotionAlarm.Models;
using System.Text.Json;

namespace MotionAlarm.Data.Queries;

public class SettingsQueryService
{
    private const int SettingsRowId = 1;
    private readonly IDbContextFactory<AlarmDbContext> _factory;

    public SettingsQueryService(IDbContextFactory<AlarmDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<AlarmSettings> GetAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.Settings.FirstOrDefaultAsync(x => x.Id == SettingsRowId, ct);

        if (row is null || string.IsNullOrWhiteSpace(row.Json))
        {
            return new AlarmSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<AlarmSettings>(row.Json) ?? new AlarmSettings();
        }
        catch (Exception)
        {
            return new AlarmSettings();
        }
    }
}
