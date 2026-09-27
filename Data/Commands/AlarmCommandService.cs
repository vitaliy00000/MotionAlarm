using Microsoft.EntityFrameworkCore;
using MotionAlarm.Data.Entities;

namespace MotionAlarm.Data.Commands;

public class AlarmCommandService
{
    private readonly IDbContextFactory<AlarmDbContext> _factory;

    public AlarmCommandService(IDbContextFactory<AlarmDbContext> factory)
    {
        _factory = factory;
    }

    public async Task LogMotionAsync(
        DateTime triggeredAtUtc,
        bool sirenEnabled,
        int sensitivity,
        CancellationToken ct = default)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var utc = triggeredAtUtc.Kind == DateTimeKind.Utc
                ? triggeredAtUtc
                : triggeredAtUtc.ToUniversalTime();

            var minuteStart = new DateTime(
                utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc);

            var minuteEnd = minuteStart.AddMinutes(1);

            var existing = await db.AlarmEvents
                .FirstOrDefaultAsync(
                    x => x.TriggeredAtUtc >= minuteStart && x.TriggeredAtUtc < minuteEnd,
                    ct);

            if (existing is not null)
            {
                existing.TriggerCount++;
                existing.ConcurrencyToken = Guid.NewGuid();
            }
            else
            {
                db.AlarmEvents.Add(new AlarmEventEntity
                {
                    TriggeredAtUtc = minuteStart,
                    TriggerCount = 1,
                    SirenEnabled = sirenEnabled,
                    Sensitivity = sensitivity,
                    ConcurrencyToken = Guid.NewGuid()
                });
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
        }
    }
}