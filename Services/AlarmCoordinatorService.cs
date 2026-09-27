using MotionAlarm.Data.Commands;
using MotionAlarm.Data.Queries;
using MotionAlarm.Models;

namespace MotionAlarm.Services;

public enum AlarmRuntimeState { Disabled, Armed, MotionDetected, Sounding }

public readonly record struct AlarmStatus(
    AlarmRuntimeState State,
    string Message,
    bool IsArmed,
    bool IsAlarmSounding);

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
    private AlarmStatus _status = new(AlarmRuntimeState.Disabled, "Охорону вимкнено", false, false);

    public event Action<AlarmStatus>? StatusChanged;

    public AlarmStatus GetStatus() => _status;

    public AlarmCoordinatorService(SettingsQueryService settings, AlarmCommandService alarmCommandService, IAlarmPlayer player, MotionDetectorService detector)
    {
        _settings = settings;
        _alarmCommandService = alarmCommandService;
        _player = player;
        _detector = detector;
    }

    public async Task ArmAsync(CancellationToken ct = default)
    {
        _current = await _settings.GetAsync(ct);
        _detector.Reset();
        _armed = true;
        Publish(AlarmRuntimeState.Armed, "Охорону увімкнено", isArmed: true, isAlarmSounding: false);
    }

    public async Task DisarmAsync(CancellationToken ct = default)
    {
        _armed = false;
        _detector.Reset();
        await _player.StopAsync(ct);
        Publish(AlarmRuntimeState.Disabled, "Охорону вимкнено", isArmed: false, isAlarmSounding: false);
    }

    public async Task StopSirenAsync(CancellationToken ct = default)
    {
        await _player.StopAsync(ct);
        _detector.Reset();
        Publish(_armed ? AlarmRuntimeState.Armed : AlarmRuntimeState.Disabled,
            _armed ? "Охорону увімкнено" : "Охорону вимкнено",
            isArmed: _armed,
            isAlarmSounding: false);
    }

    public async Task HandleSensorAsync(float accelDelta, float gyroMagnitude, DateTimeOffset nowUtc, CancellationToken ct = default)
    {
        if (!_armed)
        {
            return;
        }

        await _gate.WaitAsync(ct);

        try
        {
            (float accelThreshold, float gyroThreshold, int triggerWindowMs) = MapSensitivityProfile(
                _current.Sensitivity,
                _current.TriggerWindowMs);

            bool triggered = _detector.Process(
                accelDelta,
                gyroMagnitude,
                accelThreshold,
                gyroThreshold,
                triggerWindowMs,
                nowUtc);

            if (!triggered)
            {
                return;
            }

            const int MotionCooldownSeconds = 5;

            if (nowUtc - _lastLog >= TimeSpan.FromSeconds(MotionCooldownSeconds))
            {
                await _alarmCommandService.LogMotionAsync(
                    nowUtc.UtcDateTime,
                    _current.SirenEnabled,
                    _current.Sensitivity,
                    ct);

                _lastLog = nowUtc;
            }

            Publish(AlarmRuntimeState.MotionDetected, "Виявлено рух!", _armed, false);

            if (_current.SirenEnabled)
            {
                await _player.PlayLoopAsync(ct);
                Publish(AlarmRuntimeState.Sounding, "Виявлено рух! Сигналізація активна.", _armed, true);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private static (float accel, float gyro, int triggerWindowMs) MapSensitivityProfile(int level, int defaultWindowMs)
    {
        return Math.Clamp(level, 1, 5) switch
        {
            1 => (3.0f, 2.0f, defaultWindowMs),
            2 => (2.0f, 1.3f, defaultWindowMs),
            3 => (1.2f, 0.7f, defaultWindowMs),
            4 => (0.5f, 0.3f, defaultWindowMs),

            // maximum possible sensitivity (will increase false positives)
            5 => (0.08f, 0.04f, 80),

            _ => (0.5f, 0.3f, defaultWindowMs)
        };
    }
    private void Publish(AlarmRuntimeState state, string message, bool isArmed, bool isAlarmSounding)
    {
        _status = new AlarmStatus(state, message, isArmed, isAlarmSounding);
        StatusChanged?.Invoke(_status);
    }
}