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
    [ObservableProperty] private bool _isLoading;

    public bool ShowLoadMore => !IsLoading && !IsEndOfList;

    partial void OnIsEndOfListChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowLoadMore));
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowLoadMore));
    }

    public AlarmLogPageModel(AlarmQueryService alarmQueryService)
    {
        _alarmQueryService = alarmQueryService;
    }

    private static string FormatSummary(object count) => $"Спрацювань сьогодні: {count}";

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            _loadedCount = 0;
            IsEndOfList = false;

            var totalCountTask =
                _alarmQueryService.GetCountAsync();

            var pageTask =
                _alarmQueryService.GetPageAsync(
                    0,
                    PageSize);

            var summaryTask =
                _alarmQueryService.GetSummaryAsync(
                    DateTime.Now);

            await Task.WhenAll(
                totalCountTask,
                pageTask,
                summaryTask);

            var totalCount = await totalCountTask;
            var page = await pageTask;
            var summary = await summaryTask;

            Items = new ObservableCollection<AlarmEvent>(page);

            _loadedCount = page.Count;

            IsEndOfList = _loadedCount >= totalCount;

            Summary = FormatSummary(summary.Today);
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
                return;
            }

            foreach (var item in page)
            {
                Items.Add(item);
            }

            _loadedCount += page.Count;
            IsEndOfList = page.Count < PageSize;
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