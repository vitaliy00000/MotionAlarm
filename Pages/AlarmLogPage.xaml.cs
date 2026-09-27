namespace MotionAlarm.Pages;

public partial class AlarmLogPage : ContentPage
{
    private readonly AlarmLogPageModel _pageModel;

    public AlarmLogPage(AlarmLogPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = _pageModel = pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.LoadCommand.ExecuteAsync(null);
    }
}