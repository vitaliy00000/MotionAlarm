using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MotionAlarm.PageModels;

public partial class HomePageModel : ObservableObject, IDisposable
{
    private readonly IAlarmPlatformService _platform;
    private readonly AlarmCoordinatorService _coordinator;
    private bool _disposed;

    [ObservableProperty] private bool isArmed;
    [ObservableProperty] private bool isServiceRunning;
    [ObservableProperty] private bool isAlarmSounding;
    [ObservableProperty] private string statusMessage = "Охорону вимкнено";

    public HomePageModel(IAlarmPlatformService platform, AlarmCoordinatorService coordinator)
    {
        _platform = platform;
        _coordinator = coordinator;

        _coordinator.StatusChanged += OnStatusChanged;

        // important: sync current app-wide state immediately
        ApplyStatus(_coordinator.GetStatus());
        IsServiceRunning = _platform.IsMonitoring;
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

            IsServiceRunning = _platform.IsMonitoring;
            return;
        }

        var permission = await Permissions.RequestAsync<NotificationPermission>();
        if (permission != PermissionStatus.Granted)
        {
            StatusMessage = "Дозвіл на сповіщення не надано. Неможливо запустити моніторинг.";
            return;
        }

        var started = await _platform.StartMonitoringAsync();
        if (!started)
        {
            StatusMessage = "Не вдалося запустити службу моніторингу.";
            return;
        }

        await _coordinator.ArmAsync();

        IsServiceRunning = _platform.IsMonitoring;
    }

    [RelayCommand]
    public async Task StopAlarmSoundAsync()
    {
        await _coordinator.StopSirenAsync();
        IsAlarmSounding = false;
    }

    private void OnStatusChanged(AlarmStatus status)
            => MainThread.BeginInvokeOnMainThread(() => ApplyStatus(status));

    private void ApplyStatus(AlarmStatus status)
    {
        StatusMessage = status.Message;
        IsArmed = status.IsArmed;
        IsAlarmSounding = status.IsAlarmSounding;
    }
}