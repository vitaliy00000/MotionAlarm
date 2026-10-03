namespace MotionAlarm.Pages;

public partial class HomePage : ContentPage
{
    private readonly HomePageModel _pageModel;

    public HomePage(HomePageModel pageModel)
    {
        InitializeComponent();

        _pageModel = pageModel;
        BindingContext = _pageModel;
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            _pageModel.Dispose();
        }

        base.OnHandlerChanging(args);
    }
}