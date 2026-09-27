namespace MotionAlarm;

public sealed class NotificationPermission : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new[] { (Android.Manifest.Permission.PostNotifications, true) };
#endif
}