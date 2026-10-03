using MotionAlarm.Data.Queries;
using MotionAlarm.Localization;
using MotionAlarm.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MotionAlarm.PageModels;

public sealed class AlarmLogPageModel : PageModel
{
    private const int PageSize = 10;

    private readonly AlarmQueryService _alarmQueryService;

    private int _loadedCount;
    private ObservableCollection<AlarmEvent> _items = new();
    private string _summary = FormatSummary("-");
    private bool _isRefreshing;
    private bool _isEndOfList;
    private bool _isLoading;

    public ObservableCollection<AlarmEvent> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public string Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (SetProperty(ref _isRefreshing, value))
            {
                UpdateCommandStates();
            }
        }
    }

    public bool IsEndOfList
    {
        get => _isEndOfList;
        set
        {
            if (SetProperty(ref _isEndOfList, value))
            {
                OnPropertyChanged(nameof(ShowLoadMore));
                UpdateCommandStates();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(ShowLoadMore));
                UpdateCommandStates();
            }
        }
    }

    public bool ShowLoadMore => !IsLoading && !IsEndOfList;

    public ICommand LoadMoreCommand { get; }
    public ICommand RefreshCommand { get; }

    public AlarmLogPageModel(AlarmQueryService alarmQueryService)
    {
        _alarmQueryService = alarmQueryService;

        LoadMoreCommand = new Command(
            async () => await LoadMoreAsync(),
            () => !IsLoading && !IsEndOfList);

        RefreshCommand = new Command(
            async () => await RefreshAsync(),
            () => !IsLoading);
    }

    private static string FormatSummary(object count)
        => LocalizationResources.Instance.Format("AlarmLog_TodayCount", count);

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

            var totalCountTask = _alarmQueryService.GetCountAsync();
            var pageTask = _alarmQueryService.GetPageAsync(0, PageSize);
            var summaryTask = _alarmQueryService.GetSummaryAsync(DateTime.Now);

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

    private async Task LoadMoreAsync()
    {
        if (IsLoading || IsEndOfList)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var page = await _alarmQueryService.GetPageAsync(
                _loadedCount,
                PageSize);

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

    private void UpdateCommandStates()
    {
        ReevaluateCommand(LoadMoreCommand);

        ReevaluateCommand(RefreshCommand);
    }
}