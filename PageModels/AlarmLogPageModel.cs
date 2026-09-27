using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MotionAlarm.Data.Queries;
using MotionAlarm.Models;

namespace MotionAlarm.PageModels;

public partial class AlarmLogPageModel : ObservableObject
{
    private bool _isLoading;

    private readonly AlarmQueryService _alarmQueryService;

    [ObservableProperty] private List<AlarmEvent> items = new();
    [ObservableProperty] private string summary = "Усього спрацювань: - | Сьогодні: -";
    [ObservableProperty] private bool isRefreshing;

    public AlarmLogPageModel(AlarmQueryService alarmQueryService) => _alarmQueryService = alarmQueryService;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_isLoading)
        {
            return;
        }

        try
        {
            _isLoading = true;

            Items = (await _alarmQueryService.GetAllAsync()).ToList();
            var s = await _alarmQueryService.GetSummaryAsync(DateTime.Now);
            Summary = $"Усього спрацювань: {s.Total} | Сьогодні: {s.Today}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    
    [RelayCommand] 
    private async Task RefreshAsync() 
    {
        if(_isLoading)
        {
            return;
        }

        try 
        {
            IsRefreshing = true;
            await LoadAsync();
        }
        finally 
        {
            IsRefreshing = false; 
        }
    }
}
