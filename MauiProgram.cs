namespace MotionAlarm
{
    public static partial class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

            builder.Services.ConfigureDatabase();

            // Pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AlarmLogPage>();

            // Page models
            builder.Services.AddTransient<HomePageModel>();
            builder.Services.AddTransient<SettingsPageModel>();
            builder.Services.AddTransient<AlarmLogPageModel>();

            // Application services
            builder.Services.AddSingleton<MotionDetectorService>();
            builder.Services.AddSingleton<AlarmCoordinatorService>();

            ConfigurePlatformServices(builder.Services);

            var app = builder.Build();

            app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync().GetAwaiter().GetResult();

            return app;
        }

        static partial void ConfigurePlatformServices(IServiceCollection services);
    }
}
