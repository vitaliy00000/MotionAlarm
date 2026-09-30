using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MotionAlarm.Data.Commands;
using MotionAlarm.Data.Queries;
using MotionAlarm.Models;

namespace MotionAlarm.PageModels;

public partial class SettingsPageModel : ObservableObject
{
    private readonly SettingsQueryService _settingsQueryService;
    private readonly SettingsCommandService _settingsCommandService;
    private readonly NotificationService _notificationService;
    private AlarmSettings _settings = new();

    [ObservableProperty] public partial bool SirenEnabled { get; set; }
    [ObservableProperty] public partial double Sensitivity { get; set; } = 5;

    public SettingsPageModel(
        SettingsQueryService settingsQueryService,
        SettingsCommandService settingsCommandService,
        NotificationService notificationService)
    {
        _settingsQueryService = settingsQueryService;
        _settingsCommandService = settingsCommandService;
        _notificationService = notificationService;
    }

    public async Task LoadAsync()
    {
        _settings = await _settingsQueryService.GetAsync();
        SirenEnabled = _settings.SirenEnabled;
        Sensitivity = _settings.Sensitivity;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        _settings.SirenEnabled = SirenEnabled;
        _settings.Sensitivity = Math.Clamp((int)Math.Round(Sensitivity), 1, 5);

        await _settingsCommandService.SaveAsync(_settings);

        await _notificationService.ShowToastAsync("Налаштування збережено");
    }
}
