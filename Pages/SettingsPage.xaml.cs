namespace MotionAlarm.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsPageModel _pageModel;

    public SettingsPage(SettingsPageModel pageModel)
    {
        InitializeComponent();

        _pageModel = pageModel;
        BindingContext = _pageModel;
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