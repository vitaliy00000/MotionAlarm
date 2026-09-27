using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using AndroidX.Core.App;

namespace MotionAlarm;

[Service(Name = "com.companyname.motionalarm.MotionMonitoringService",
    Exported = false,
    ForegroundServiceType = Android.Content.PM.ForegroundService.TypeSpecialUse)]
public class MotionMonitoringService : Service, ISensorEventListener
{
    public const string ActionStart = "mationalarm.action.START";
    public const string ActionStop = "mationalarm.action.STOP";
    private const string ChannelId = "motion_alarm_channel";
    private const int NotificationId = 7010;

    private SensorManager? _sensorManager;
    private Sensor? _accelerometer;
    private Sensor? _gyroscope;
    private AlarmCoordinatorService _coordinator;
    private float _accelDelta;
    private float _gyroMagnitude;
    private bool _started;

    public override void OnCreate()
    {
        base.OnCreate();
        _sensorManager = (SensorManager?)GetSystemService(SensorService);
        _accelerometer = _sensorManager?.GetDefaultSensor(SensorType.Accelerometer);
        _gyroscope = _sensorManager?.GetDefaultSensor(SensorType.Gyroscope);

        _coordinator = IPlatformApplication.Current?.Services?.GetService<AlarmCoordinatorService>();
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var action = intent?.Action ?? ActionStart;
        if (action == ActionStop)
        {
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        EnsureNotificationChannel();
        StartForeground(NotificationId, BuildNotification("Motion protection active"));

        if (!_started)
        {
            if (_accelerometer is not null)
            {
                _sensorManager?.RegisterListener(this, _accelerometer, SensorDelay.Game);
            }

            if (_gyroscope is not null)
            {
                _sensorManager?.RegisterListener(this, _gyroscope, SensorDelay.Game);
            }

            _started = true;
        }

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        _sensorManager?.UnregisterListener(this);
        _started = false;
        base.OnDestroy();
    }

    public override IBinder? OnBind(Intent? intent) => null;

    public void OnSensorChanged(SensorEvent? e)
    {
        if (e?.Sensor is null || _coordinator is null)
        {
            return;
        }

        if (e.Sensor.Type == SensorType.Accelerometer)
        {
            var x = e.Values[0];
            var y = e.Values[1];
            var z = e.Values[2];
            var magnitude = MathF.Sqrt(x * x + y * y + z * z);
            _accelDelta = MathF.Abs(magnitude - SensorManager.GravityEarth);
        }
        else if (e.Sensor.Type == SensorType.Gyroscope)
        {
            var x = e.Values[0];
            var y = e.Values[1];
            var z = e.Values[2];
            _gyroMagnitude = MathF.Sqrt(x * x + y * y + z * z);
        }

        _ = _coordinator.HandleSensorAsync(_accelDelta, _gyroMagnitude, DateTimeOffset.UtcNow);
    }

    public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy) { }

    private void EnsureNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var manager = (NotificationManager)GetSystemService(NotificationService)!;
        if (manager.GetNotificationChannel(ChannelId) is not null)
        {
            return;
        }

        var channel = new NotificationChannel(ChannelId, "Motion alarm monitoring", NotificationImportance.Low)
        {
            Description = "Foreground monitoring for anti-theft alarm."
        };

        manager.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification(string text)
    {
        var launchIntent = PackageManager?.GetLaunchIntentForPackage(PackageName);
        var pendingIntent = PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        return new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("Motion Alarm")
            .SetContentText(text)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetOngoing(true)
            .SetContentIntent(pendingIntent)
            .Build();
    }
}