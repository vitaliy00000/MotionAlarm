using MotionAlarm.Localization;
using System.Globalization;

namespace MotionAlarm;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Dark;

        InitializeLocalization();
    }

    private static void InitializeLocalization()
    {
        var systemCulture = CultureInfo.CurrentUICulture;
        var culture = SupportedCultures.Resolve(systemCulture);

        LocalizationResources.Instance.SetCulture(culture);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}