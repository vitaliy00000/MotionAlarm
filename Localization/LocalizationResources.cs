using MotionAlarm.Resources.Localization;
using System.ComponentModel;
using System.Globalization;

namespace MotionAlarm.Localization;

public sealed class LocalizationResources : INotifyPropertyChanged
{
    public static LocalizationResources Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public CultureInfo CurrentCulture { get; private set; }
        = CultureInfo.CurrentUICulture;

    public string this[string key] =>
        AppResources.ResourceManager.GetString(key, CurrentCulture) ?? key;

    public string Format(string key, params object[] args)
    {
        var format = this[key];

        return string.Format(
            CurrentCulture,
            format,
            args);
    }

    public void SetCulture(CultureInfo culture)
    {
        CurrentCulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        AppResources.Culture = culture;

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(null));
    }
}