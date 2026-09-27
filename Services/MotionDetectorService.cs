namespace MotionAlarm.Services;

public sealed class MotionDetectorService
{
    private DateTimeOffset? _movementStarted;

    public bool Process(
        float accelDelta,
        float gyroMagnitude,
        float accelThreshold,
        float gyroThreshold,
        int triggerWindowMs,
        DateTimeOffset nowUtc)
    {
        bool movementDetected =
            accelDelta >= accelThreshold ||
            gyroMagnitude >= gyroThreshold;

        if (!movementDetected)
        {
            _movementStarted = null;
            return false;
        }

        _movementStarted ??= nowUtc;

        if ((nowUtc - _movementStarted.Value).TotalMilliseconds < triggerWindowMs)
        {
            return false;
        }

        _movementStarted = null;

        return true;
    }

    public void Reset()
    {
        _movementStarted = null;
    }
}