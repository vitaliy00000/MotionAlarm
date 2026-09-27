namespace MotionAlarm.Services;

public interface IAlarmPlayer
{
    Task PlayLoopAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

public interface IAlarmPlatformService
{
    Task<bool> StartMonitoringAsync(CancellationToken ct = default);
    Task StopMonitoringAsync(CancellationToken ct = default);
    bool IsMonitoring { get; }
}

public sealed class NoOpAlarmPlayer : IAlarmPlayer
{
    public Task PlayLoopAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class NoOpAlarmPlatformService : IAlarmPlatformService
{
    public bool IsMonitoring => false;
    public Task<bool> StartMonitoringAsync(CancellationToken ct = default) => Task.FromResult(false);
    public Task StopMonitoringAsync(CancellationToken ct = default) => Task.CompletedTask;
}
