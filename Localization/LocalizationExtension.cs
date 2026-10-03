namespace MotionAlarm.Localization;

[AcceptEmptyServiceProvider]
[ContentProperty(nameof(Key))]
public class LocalizationExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public string? StringFormat { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationResources.Instance,
            StringFormat = StringFormat
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        ProvideValue(serviceProvider);
}