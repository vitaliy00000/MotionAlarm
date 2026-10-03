using MotionAlarm.Resources.Localization;

namespace MotionAlarm.Pages.Components;

public partial class LoadingOverlay : Grid
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(LoadingOverlay),
        AppResources.Common_Loading);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public LoadingOverlay()
    {
        InitializeComponent();
    }
}