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
            (float accelThreshold, float gyroThreshold) = MapSensitivity(_current.Sensitivity);

            bool triggered = _detector.Process(
                accelDelta,
                gyroMagnitude,
                accelThreshold,
                gyroThreshold,
                _current.TriggerWindowMs,
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

    private void Publish(AlarmRuntimeState state, string message, bool isArmed, bool isAlarmSounding)
    {
        _status = new AlarmStatus(state, message, isArmed, isAlarmSounding);
        StatusChanged?.Invoke(_status);
    }

    private static (float accel, float gyro) MapSensitivity(int level)
    {
        return Math.Clamp(level, 1, 5) switch
        {
            1 => (4.0f, 2.5f), // Very low
            2 => (3.0f, 2.0f), // Low
            3 => (2.2f, 1.5f), // Medium
            4 => (1.6f, 1.1f), // High
            5 => (1.2f, 0.8f), // Very high
            _ => (2.2f, 1.5f)
        };
    }
}