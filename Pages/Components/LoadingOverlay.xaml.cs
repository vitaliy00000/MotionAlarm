namespace MotionAlarm.Pages.Components;

public partial class LoadingOverlay : Grid
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(LoadingOverlay),
            string.Empty,
            propertyChanged: OnTextChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public LoadingOverlay()
    {
        InitializeComponent();

        UpdateTextVisibility();
    }

    private static void OnTextChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is LoadingOverlay view)
        {
            view.UpdateTextVisibility();
        }
    }

    private void UpdateTextVisibility()
    {
        label.IsVisible = !string.IsNullOrWhiteSpace(Text);
    }
}