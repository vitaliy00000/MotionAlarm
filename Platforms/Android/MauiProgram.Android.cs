using MotionAlarm.Abstractions;

namespace MotionAlarm;

public static partial class MauiProgram
{
    static partial void ConfigurePlatformServices(IServiceCollection services)
    {
        services.AddSingleton<IAlarmPlayer, AlarmPlayer>();
        services.AddSingleton<IAlarmPlatformService, AlarmPlatformService>();
    }
}
