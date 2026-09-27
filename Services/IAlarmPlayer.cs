namespace MotionAlarm.Services;

public interface IAlarmPlayer
{
    Task PlayLoopAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}