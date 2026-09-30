using Android.Content;
using MotionAlarm.Abstractions;

namespace MotionAlarm;

public sealed class AlarmPlatformService : IAlarmPlatformService
{
    public bool IsMonitoring { get; private set; }

    public Task<bool> StartMonitoringAsync(CancellationToken ct = default)
    {
        var context = Android.App.Application.Context;
        var intent = new Intent(context, typeof(MotionMonitoringService));
        intent.SetAction(MotionMonitoringService.ActionStart);

        context.StartForegroundService(intent);
        IsMonitoring = true;
        return Task.FromResult(true);
    }

    public Task StopMonitoringAsync(CancellationToken ct = default)
    {
        var context = Android.App.Application.Context;
        var intent = new Intent(context, typeof(MotionMonitoringService));
        intent.SetAction(MotionMonitoringService.ActionStop);
        context.StartService(intent);

        IsMonitoring = false;
        return Task.CompletedTask;
    }
}
