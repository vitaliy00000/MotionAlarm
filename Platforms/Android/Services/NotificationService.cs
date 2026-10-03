using Android.Widget;
using MotionAlarm.Abstractions;

namespace MotionAlarm;

public sealed class NotificationService : INotificationService
{
    public Task ShowToastAsync(string message)
    {
        Toast.MakeText(
            Android.App.Application.Context,
            message,
            ToastLength.Long)?.Show();

        return Task.CompletedTask;
    }
}
