namespace MotionAlarm;

public sealed class NotificationPermission : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(33)
            ? new[]
            {
                (Android.Manifest.Permission.PostNotifications, true)
            }
            : Array.Empty<(string androidPermission, bool isRuntime)>();
#endif
}