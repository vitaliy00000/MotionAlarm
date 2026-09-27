using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;

namespace MotionAlarm.Services;

public class NotificationService
{
    public async Task ShowSnackbarAsync(string message)
    {
        var options = new SnackbarOptions
        {
            BackgroundColor = Color.FromArgb("#FF3300"),
            TextColor = Colors.White,
            ActionButtonTextColor = Colors.Yellow,
            CornerRadius = new CornerRadius(0),
            Font = Font.SystemFontOfSize(18),
            ActionButtonFont = Font.SystemFontOfSize(14)
        };

        var snackbar = Snackbar.Make(
            message,
            visualOptions: options);

        await snackbar.Show();
    }

    public async Task ShowToastAsync(string message)
    {
        var toast = Toast.Make(
            message,
            textSize: 18);

        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(5));

        await toast.Show(cts.Token);
    }
}