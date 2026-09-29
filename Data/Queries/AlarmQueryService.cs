using Microsoft.EntityFrameworkCore;
using MotionAlarm.Models;

namespace MotionAlarm.Data.Queries;

public class AlarmQueryService
{
    private readonly IDbContextFactory<AlarmDbContext> _factory;

    public AlarmQueryService(IDbContextFactory<AlarmDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<AlarmEvent>> GetPageAsync(
        int skip,
        int take,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var events = await db.AlarmEvents
            .OrderByDescending(x => x.TriggeredAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(x => new AlarmEvent
            {
                Id = x.Id,
                TriggeredAtUtc = x.TriggeredAtUtc,
                TriggerCount = x.TriggerCount,
                SirenEnabled = x.SirenEnabled,
                Sensitivity = x.Sensitivity
            })
            .ToListAsync(ct);

        foreach (var alarmEvent in events)
        {
            alarmEvent.TriggeredAtUtc =
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(alarmEvent.TriggeredAtUtc, DateTimeKind.Utc),
                    TimeZoneInfo.Local);
        }

        return events;
    }

    public async Task<int> GetCountAsync(
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        return await db.AlarmEvents.CountAsync(ct);
    }

    public async Task<(int Total, int Today, int ThisWeek)> GetSummaryAsync(
        DateTime nowLocal,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var total = await db.AlarmEvents
            .Select(x => (int?)x.TriggerCount)
            .SumAsync(ct) ?? 0;

        var todayStartUtc = nowLocal.Date.ToUniversalTime();

        var today = await db.AlarmEvents
            .Where(x => x.TriggeredAtUtc >= todayStartUtc)
            .Select(x => (int?)x.TriggerCount)
            .SumAsync(ct) ?? 0;

        var diff = ((int)nowLocal.DayOfWeek + 6) % 7;
        var weekStartUtc = nowLocal.Date.AddDays(-diff).ToUniversalTime();

        var thisWeek = await db.AlarmEvents
            .Where(x => x.TriggeredAtUtc >= weekStartUtc)
            .Select(x => (int?)x.TriggerCount)
            .SumAsync(ct) ?? 0;

        return (total, today, thisWeek);
    }
}