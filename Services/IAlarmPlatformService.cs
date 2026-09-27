namespace MotionAlarm.Services;

public interface IAlarmPlatformService
{
    Task<bool> StartMonitoringAsync(CancellationToken ct = default);
    Task StopMonitoringAsync(CancellationToken ct = default);
    bool IsMonitoring { get; }
}
