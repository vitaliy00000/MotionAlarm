using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MotionAlarm.PageModels;

public partial class HomePageModel : ObservableObject, IDisposable
{
    private readonly IAlarmPlatformService _platform;
    private readonly AlarmCoordinatorService _coordinator;

    private bool _disposed;

    [ObservableProperty]
    private bool isArmed;

    [ObservableProperty]
    private string statusMessage = "Охорону вимкнено";

    public HomePageModel(
        IAlarmPlatformService platform,
        AlarmCoordinatorService coordinator)
    {
        _platform = platform;
        _coordinator = coordinator;

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

    [RelayCommand]
    public async Task ToggleAlarmAsync()
    {
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

        var started =
            await _platform.StartMonitoringAsync();

        if (!started)
        {
            StatusMessage =
                "Не вдалося запустити службу моніторингу.";

            return;
        }

        await _coordinator.ArmAsync();
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