using Microsoft.EntityFrameworkCore;
using MotionAlarm.Data.Commands;
using MotionAlarm.Data.Queries;

namespace MotionAlarm.Data;

public static class EFCoreCollectionExtensions
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services)
    {
        services.AddDbContextFactory<AlarmDbContext>(options =>
            options.UseSqlite($"Data Source={Constants.DatabasePath}"));

        services.AddSingleton<DatabaseInitializer>();

        services.AddTransient<SettingsCommandService>();
        services.AddTransient<SettingsQueryService>();

        services.AddTransient<AlarmCommandService>();
        services.AddTransient<AlarmQueryService>();

        return services;
    }
}
