namespace MotionAlarm.Services;

public sealed class MotionDetectorService
{
    private DateTimeOffset? _movementStarted;

    private float _baselineX;
    private float _baselineY;
    private float _baselineZ;
    private bool _hasBaseline;

    public bool Process(
        float accelX,
        float accelY,
        float accelZ,
        float accelDelta,
        float gyroMagnitude,
        float accelThreshold,
        float gyroThreshold,
        int triggerWindowMs,
        float orientationThresholdDegrees,
        DateTimeOffset nowUtc)
    {
        // Establish the initial device orientation.
        // The first valid accelerometer sample becomes the baseline.
        if (!_hasBaseline)
        {
            SetBaseline(accelX, accelY, accelZ);
            return false;
        }

        // Primary movement detection.
        bool accelerationDetected =
            accelDelta >= accelThreshold;

        // Secondary movement detection.
        // Gyroscope is optional and can be unavailable on some devices.
        bool rotationDetected =
            gyroMagnitude >= gyroThreshold;

        // Primary detection for slow lifting/tilting.
        float orientationChange =
            GetOrientationChangeDegrees(
                accelX,
                accelY,
                accelZ);

        bool orientationChanged =
            orientationChange >= orientationThresholdDegrees;

        bool movementDetected =
            accelerationDetected ||
            orientationChanged ||
            rotationDetected;

        if (!movementDetected)
        {
            _movementStarted = null;
            return false;
        }

        _movementStarted ??= nowUtc;

        // Movement must remain detected continuously
        // for the configured trigger window.
        if ((nowUtc - _movementStarted.Value).TotalMilliseconds
            < triggerWindowMs)
        {
            return false;
        }

        _movementStarted = null;

        if (orientationChanged)
        {
            // The device may remain stationary in the new position.
            // Update the baseline value so that this same position
            // is not re-detected as movement.
            SetBaseline(
                accelX,
                accelY,
                accelZ);
        }

        return true;
    }

    public void Reset()
    {
        _movementStarted = null;

        _baselineX = 0f;
        _baselineY = 0f;
        _baselineZ = 0f;

        _hasBaseline = false;
    }

    private void SetBaseline(
        float x,
        float y,
        float z)
    {
        float length = MathF.Sqrt(
            x * x +
            y * y +
            z * z);

        if (length < 0.001f)
        {
            return;
        }

        _baselineX = x / length;
        _baselineY = y / length;
        _baselineZ = z / length;

        _hasBaseline = true;
    }

    private float GetOrientationChangeDegrees(
        float x,
        float y,
        float z)
    {
        float length = MathF.Sqrt(
            x * x +
            y * y +
            z * z);

        if (length < 0.001f)
        {
            return 0f;
        }

        // Normalize current gravity vector.
        x /= length;
        y /= length;
        z /= length;

        // Compare current gravity direction
        // with the original device orientation.
        float dot =
            _baselineX * x +
            _baselineY * y +
            _baselineZ * z;

        dot = Math.Clamp(dot, -1f, 1f);

        return MathF.Acos(dot) *
               (180f / MathF.PI);
    }
}