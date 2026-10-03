namespace MotionAlarm.Pages;

public partial class AlarmLogPage : ContentPage
{
    private readonly AlarmLogPageModel _pageModel;

    public AlarmLogPage(AlarmLogPageModel viewModel)
    {
        InitializeComponent();

        BindingContext = _pageModel = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        _pageModel.CurrentState = PageModel.STATE_LOADING;

        // Allow MAUI to render the loading state before continuing.
        // Workaround for MAUI UI rendering timing issue.
        await Task.Yield();

        await _pageModel.LoadAsync();
    }
}