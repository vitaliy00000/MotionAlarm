using MotionAlarm.Abstractions;
using MotionAlarm.Data.Queries;
using System.Windows.Input;

namespace MotionAlarm.PageModels;

public sealed class HomePageModel : PageModel, IDisposable
{
    private readonly IAlarmPlatformService _platform;
    private readonly AlarmCoordinatorService _coordinator;
    private readonly SettingsQueryService _settingsQueryService;

    private bool _disposed;
    private bool _isArmed;
    private string _statusMessage = "Охорону вимкнено";
    private bool _isArming;
    private string _armCountdownText = string.Empty;

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

    public string ArmCountdownText
    {
        get => _armCountdownText;
        set
        {
            if (SetProperty(ref _armCountdownText, value))
            {
                OnPropertyChanged(nameof(HasArmCountdown));
            }
        }
    }

    public bool HasArmCountdown =>
        !string.IsNullOrWhiteSpace(ArmCountdownText);

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
                StatusMessage =
                    "Дозвіл на сповіщення не надано. Неможливо запустити моніторинг.";

                return;
            }

            var settings = await _settingsQueryService.GetAsync();

            var started = await _platform.StartMonitoringAsync();

            if (!started)
            {
                StatusMessage =
                    "Не вдалося запустити службу моніторингу.";

                return;
            }

            if (settings.ArmDelaySeconds > 0)
            {
                StatusMessage = "Охорону буде увімкнено";

                for (var seconds = settings.ArmDelaySeconds;
                     seconds > 0;
                     seconds--)
                {
                    ArmCountdownText = seconds.ToString("00");

                    await Task.Delay(1000);
                }

                ArmCountdownText = string.Empty;
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
        StatusMessage = status.Message;
        IsArmed = status.IsArmed;

        UpdateCommandState();
    }

    private void UpdateCommandState()
    {
        ReevaluateCommand(ToggleAlarmCommand);
    }
}