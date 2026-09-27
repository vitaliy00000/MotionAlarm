using CommunityToolkit.Maui.Alerts;

namespace MotionAlarm.Services;

public class NotificationService
{
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