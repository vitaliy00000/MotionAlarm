namespace MotionAlarm.Pages;

public partial class AlarmLogPage : ContentPage
{
    private readonly AlarmLogPageModel _pageModel;

    public AlarmLogPage(AlarmLogPageModel pageModel)
    {
        InitializeComponent();

        _pageModel = pageModel;
        BindingContext = _pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _pageModel.LoadAsync();
    }
}