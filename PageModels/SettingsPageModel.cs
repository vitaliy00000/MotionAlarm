using MotionAlarm.Abstractions;
using MotionAlarm.Data.Commands;
using MotionAlarm.Data.Queries;
using MotionAlarm.Models;
using System.Windows.Input;

namespace MotionAlarm.PageModels;

public sealed class SettingsPageModel : PageModel
{
    private readonly SettingsQueryService _settingsQueryService;
    private readonly SettingsCommandService _settingsCommandService;
    private readonly INotificationService _notificationService;

    private AlarmSettings _settings = new();

    private bool _sirenEnabled;
    private double _sensitivity = 5;
    private double _armDelaySeconds;
    private double _soundDelaySeconds;

    public bool SirenEnabled
    {
        get => _sirenEnabled;
        set => SetProperty(ref _sirenEnabled, value);
    }

    public double Sensitivity
    {
        get => _sensitivity;
        set => SetProperty(ref _sensitivity, value);
    }

    public double ArmDelaySeconds
    {
        get => _armDelaySeconds;
        set => SetProperty(ref _armDelaySeconds, value);
    }

    public double SoundDelaySeconds
    {
        get => _soundDelaySeconds;
        set => SetProperty(ref _soundDelaySeconds, value);
    }

    public ICommand SaveCommand { get; }

    public SettingsPageModel(
        SettingsQueryService settingsQueryService,
        SettingsCommandService settingsCommandService,
        INotificationService notificationService)
    {
        _settingsQueryService = settingsQueryService;
        _settingsCommandService = settingsCommandService;
        _notificationService = notificationService;

        SaveCommand = new Command(
            async () => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        _settings = await _settingsQueryService.GetAsync();

        SirenEnabled = _settings.SirenEnabled;
        Sensitivity = _settings.Sensitivity;
        ArmDelaySeconds = _settings.ArmDelaySeconds;
        SoundDelaySeconds = _settings.SoundDelaySeconds;
    }

    private async Task SaveAsync()
    {
        _settings.SirenEnabled = SirenEnabled;

        _settings.Sensitivity = Math.Clamp(
            (int)Math.Round(Sensitivity),
            1,
            5);

        _settings.ArmDelaySeconds = Math.Clamp(
            (int)Math.Round(ArmDelaySeconds),
            0,
            60);

        _settings.SoundDelaySeconds = Math.Clamp(
            (int)Math.Round(SoundDelaySeconds),
            0,
            60);

        await _settingsCommandService.SaveAsync(_settings);

        await _notificationService.ShowToastAsync(
            "Налаштування збережено");
    }
}