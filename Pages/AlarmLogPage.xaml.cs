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

        await _pageModel.LoadAsync();
    }
}