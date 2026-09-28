using MotionAlarm.Data.Commands;
using MotionAlarm.Data.Queries;
using MotionAlarm.Models;

namespace MotionAlarm.Services;

public enum AlarmRuntimeState
{
    Disabled,
    Armed,
    MotionDetected,
    Sounding
}

public readonly record struct AlarmStatus(
    AlarmRuntimeState State,
    string Message,
    bool IsArmed);

public sealed class AlarmCoordinatorService
{
    private readonly SettingsQueryService _settings;
    private readonly AlarmCommandService _alarmCommandService;
    private readonly IAlarmPlayer _player;
    private readonly MotionDetectorService _detector;

    private readonly SemaphoreSlim _gate = new(1, 1);

    private AlarmSettings _current = new();

    private DateTimeOffset _lastLog = DateTimeOffset.MinValue;

    private bool _armed;

    private AlarmStatus _status =
        new(
            AlarmRuntimeState.Disabled,
            "Охорону вимкнено",
            false);

    public event Action<AlarmStatus>? StatusChanged;

    public AlarmCoordinatorService(
        SettingsQueryService settings,
        AlarmCommandService alarmCommandService,
        IAlarmPlayer player,
        MotionDetectorService detector)
    {
        _settings = settings;
        _alarmCommandService = alarmCommandService;
        _player = player;
        _detector = detector;
    }

    public AlarmStatus GetStatus() => _status;

    public async Task ArmAsync(
        CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);

        try
        {
            _current = await _settings.GetAsync(ct);

            _lastLog = DateTimeOffset.MinValue;

            _detector.Reset();

            _armed = true;

            Publish(
                AlarmRuntimeState.Armed,
                "Охорону увімкнено",
                isArmed: true);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DisarmAsync(
        CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);

        try
        {
            _armed = false;

            _detector.Reset();

            await _player.StopAsync(ct);

            Publish(
                AlarmRuntimeState.Disabled,
                "Охорону вимкнено",
                isArmed: false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task HandleSensorAsync(
        float accelX,
        float accelY,
        float accelZ,
        float accelDelta,
        float gyroMagnitude,
        DateTimeOffset nowUtc,
        CancellationToken ct = default)
    {
        if (!_armed)
        {
            return;
        }

        await _gate.WaitAsync(ct);

        try
        {
            if (!_armed)
            {
                return;
            }

            (
                float accelThreshold,
                float gyroThreshold,
                int triggerWindowMs,
                float orientationThresholdDegrees
            ) = MapSensitivityProfile(
                _current.Sensitivity,
                _current.TriggerWindowMs);

            bool triggered = _detector.Process(
                accelX,
                accelY,
                accelZ,
                accelDelta,
                gyroMagnitude,
                accelThreshold,
                gyroThreshold,
                triggerWindowMs,
                orientationThresholdDegrees,
                nowUtc);

            if (!triggered)
            {
                return;
            }

            // Cooldown between processed/logged motion triggers.
            const int motionCooldownSeconds = 1;

            if (nowUtc - _lastLog >=
                TimeSpan.FromSeconds(motionCooldownSeconds))
            {
                await _alarmCommandService.LogMotionAsync(
                    nowUtc.UtcDateTime,
                    _current.SirenEnabled,
                    _current.Sensitivity,
                    ct);

                _lastLog = nowUtc;
            }

            if (_status.State is
                    AlarmRuntimeState.MotionDetected or
                    AlarmRuntimeState.Sounding)
            {
                return;
            }

            if (_current.SirenEnabled)
            {
                await _player.PlayLoopAsync(ct);

                Publish(
                    AlarmRuntimeState.Sounding,
                    "Виявлено рух! Сигналізація активна.",
                    isArmed: _armed);
            }
            else
            {
                Publish(
                    AlarmRuntimeState.MotionDetected,
                    "Виявлено рух!",
                    isArmed: _armed);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private static (
        float accel,
        float gyro,
        int triggerWindowMs,
        float orientationDegrees)
        MapSensitivityProfile(
            int level,
            int defaultWindowMs)
    {
        return Math.Clamp(level, 1, 5) switch
        {
            1 => (
                3.0f,
                2.0f,
                defaultWindowMs,
                20f),

            2 => (
                2.0f,
                1.3f,
                defaultWindowMs,
                15f),

            3 => (
                1.2f,
                0.7f,
                defaultWindowMs,
                10f),

            4 => (
                0.5f,
                0.3f,
                defaultWindowMs,
                7f),

            // Very sensitive:
            // - small acceleration
            // - small rotation
            // - only 4 degrees of orientation change
            // - 150 ms trigger window
            5 => (
                0.2f,
                0.3f,
                150,
                4f),

            _ => (
                0.5f,
                0.3f,
                defaultWindowMs,
                7f)
        };
    }

    private void Publish(
        AlarmRuntimeState state,
        string message,
        bool isArmed)
    {
        var newStatus = new AlarmStatus(
            state,
            message,
            isArmed);

        // Do not notify subscribers if the status
        // is exactly the same as the current status.
        if (newStatus == _status)
        {
            return;
        }

        _status = newStatus;

        StatusChanged?.Invoke(newStatus);
    }
}