using System.Globalization;

namespace MotionAlarm.Localization;

public static class SupportedCultures
{
    public static readonly string[] All =
    [
        "en",
        "uk"
    ];

    public static CultureInfo Resolve(CultureInfo systemCulture)
    {
        var exact = All.FirstOrDefault(
            x => string.Equals(
                x,
                systemCulture.Name,
                StringComparison.OrdinalIgnoreCase));

        if (exact != null)
        {
            return new CultureInfo(exact);
        }

        var language = All.FirstOrDefault(
            x => string.Equals(
                x,
                systemCulture.TwoLetterISOLanguageName,
                StringComparison.OrdinalIgnoreCase));

        if (language != null)
        {
            return new CultureInfo(language);
        }

        return new CultureInfo("en");
    }
}
