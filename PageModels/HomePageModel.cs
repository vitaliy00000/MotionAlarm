using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MotionAlarm.Abstractions;
using MotionAlarm.Data.Queries;

namespace MotionAlarm.PageModels;

public partial class HomePageModel : ObservableObject, IDisposable
{
    private readonly IAlarmPlatformService _platform;
    private readonly AlarmCoordinatorService _coordinator;
    private readonly SettingsQueryService _settingsQueryService;

    private bool _disposed;

    [ObservableProperty] public partial bool IsArmed { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = "Охорону вимкнено";
    [ObservableProperty] public partial bool IsArming { get; set; }
    [ObservableProperty] public partial string ArmCountdownText { get; set; } = string.Empty;

    public bool HasArmCountdown => !string.IsNullOrWhiteSpace(ArmCountdownText);

    public HomePageModel(
        IAlarmPlatformService platform,
        AlarmCoordinatorService coordinator,
        SettingsQueryService settingsQueryService)
    {
        _platform = platform;
        _coordinator = coordinator;
        _settingsQueryService = settingsQueryService;

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

    partial void OnArmCountdownTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasArmCountdown));
    }

    [RelayCommand]
    public async Task ToggleAlarmAsync()
    {
        try
        {
            IsArming = true;

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

            if(settings.ArmDelaySeconds > 0)
            {
                StatusMessage = "Охорону буде увімкнено";

                for (var seconds = settings.ArmDelaySeconds; seconds > 0; seconds--)
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
    }
}