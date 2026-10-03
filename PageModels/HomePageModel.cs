using MotionAlarm.Abstractions;
using MotionAlarm.Data.Queries;
using MotionAlarm.Localization;
using System.Windows.Input;

namespace MotionAlarm.PageModels;

public sealed class HomePageModel : PageModel, IDisposable
{
    private readonly IAlarmPlatformService _platform;
    private readonly AlarmCoordinatorService _coordinator;
    private readonly SettingsQueryService _settingsQueryService;

    private bool _disposed;
    private bool _isArmed;
    private string _statusMessage = LocalizationResources.Instance["Home_Status_Disarmed"];
    private bool _isArming;
    private string _countdownText = string.Empty;

    public bool IsArmed
    {
        get => _isArmed;
        set => SetProperty(ref _isArmed, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsArming
    {
        get => _isArming;
        set => SetProperty(ref _isArming, value);
    }

    public string CountdownText
    {
        get => _countdownText;
        set
        {
            if (SetProperty(ref _countdownText, value))
            {
                OnPropertyChanged(nameof(HasCountdown));
            }
        }
    }

    public bool HasCountdown =>
        !string.IsNullOrWhiteSpace(CountdownText);

    public ICommand ToggleAlarmCommand { get; }

    public HomePageModel(
        IAlarmPlatformService platform,
        AlarmCoordinatorService coordinator,
        SettingsQueryService settingsQueryService)
    {
        _platform = platform;
        _coordinator = coordinator;
        _settingsQueryService = settingsQueryService;

        ToggleAlarmCommand = new Command(
            async () => await ToggleAlarmAsync(),
            () => !IsArming);

        _coordinator.StatusChanged += OnStatusChanged;

        // Sync current app-wide state immediately.
        ApplyStatus(_coordinator.GetStatus());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _coordinator.StatusChanged -= OnStatusChanged;

        _disposed = true;
    }

    public async Task ToggleAlarmAsync()
    {
        if (IsArming)
        {
            return;
        }

        try
        {
            IsArming = true;
            UpdateCommandState();

            if (IsArmed)
            {
                await _platform.StopMonitoringAsync();
                await _coordinator.DisarmAsync();

                return;
            }

            // POST_NOTIFICATIONS is only a runtime permission
            // on Android 13+.
            var permission =
                await Permissions.RequestAsync<NotificationPermission>();

            if (OperatingSystem.IsAndroidVersionAtLeast(33) &&
                permission != PermissionStatus.Granted)
            {
                StatusMessage = LocalizationResources.Instance["Home_NotificationPermissionDenied"];

                return;
            }

            var settings = await _settingsQueryService.GetAsync();

            var started = await _platform.StartMonitoringAsync();

            if (!started)
            {
                StatusMessage = LocalizationResources.Instance["Home_MonitoringStartFailed"];

                return;
            }

            if (settings.ArmDelaySeconds > 0)
            {
                StatusMessage = LocalizationResources.Instance["Home_ArmingSoon"];

                for (var seconds = settings.ArmDelaySeconds;
                     seconds > 0;
                     seconds--)
                {
                    CountdownText = seconds.ToString("00");

                    await Task.Delay(1000);
                }

                CountdownText = string.Empty;
            }

            await _coordinator.ArmAsync();
        }
        finally
        {
            IsArming = false;
            UpdateCommandState();
        }
    }

    private void OnStatusChanged(AlarmStatus status)
    {
        MainThread.BeginInvokeOnMainThread(
            () => ApplyStatus(status));
    }

    private void ApplyStatus(AlarmStatus status)
    {
        StatusMessage = status.State switch
        {
            AlarmRuntimeState.Disabled =>
                LocalizationResources.Instance["Alarm_Status_Disarmed"],

            AlarmRuntimeState.Armed =>
                LocalizationResources.Instance["Alarm_Status_Armed"],

            AlarmRuntimeState.MotionDetected =>
                LocalizationResources.Instance["Alarm_MotionDetected"],

            AlarmRuntimeState.AlarmCountdown =>
                LocalizationResources.Instance["Alarm_AlarmCountdown"],

            AlarmRuntimeState.Sounding =>
                LocalizationResources.Instance["Alarm_AlarmActive"],

            _ => string.Empty
        };

        CountdownText =
            status.State == AlarmRuntimeState.AlarmCountdown
                ? status.CountdownSeconds.ToString("00")
                : string.Empty;

        IsArmed = status.IsArmed;

        UpdateCommandState();
    }

    private void UpdateCommandState()
    {
        ReevaluateCommand(ToggleAlarmCommand);
    }
}