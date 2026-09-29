using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MotionAlarm.Data.Queries;
using MotionAlarm.Models;

namespace MotionAlarm.PageModels;

public partial class AlarmLogPageModel : ObservableObject
{
    private const int PageSize = 10;
    private int _loadedCount;

    private readonly AlarmQueryService _alarmQueryService;

    [ObservableProperty] private ObservableCollection<AlarmEvent> _items = new();
    [ObservableProperty] private string _summary = FormatSummary("-");
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private bool _isEndOfList;
    [ObservableProperty] private bool _canLoadMore;
    [ObservableProperty] private bool _isLoading;

    public AlarmLogPageModel(AlarmQueryService alarmQueryService)
    {
        _alarmQueryService = alarmQueryService;
    }

    private static string FormatSummary(object count) => $"Спрацювань сьогодні: {count}";

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            _loadedCount = 0;
            IsEndOfList = false;
            CanLoadMore = false;

            var page = await _alarmQueryService.GetPageAsync(_loadedCount, PageSize);
            Items = new ObservableCollection<AlarmEvent>(page);
            _loadedCount += page.Count;

            IsEndOfList = page.Count < PageSize;
            CanLoadMore = Items.Count > 0 && !IsEndOfList;

            var s = await _alarmQueryService.GetSummaryAsync(DateTime.Now);
            Summary = FormatSummary(s.Today);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoading || IsEndOfList)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var page = await _alarmQueryService.GetPageAsync(_loadedCount, PageSize);

            if (page.Count == 0)
            {
                IsEndOfList = true;
                CanLoadMore = false;
                return;
            }

            foreach (var item in page)
            {
                Items.Add(item);
            }

            _loadedCount += page.Count;
            IsEndOfList = page.Count < PageSize;
            CanLoadMore = !IsEndOfList;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsLoading)
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