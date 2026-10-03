namespace MotionAlarm.Abstractions;

public interface INotificationService
{
    Task ShowToastAsync(string message);
}
