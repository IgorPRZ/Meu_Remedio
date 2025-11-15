using MeuRemedio.Services;
using Microsoft.Extensions.Logging;
using MR.Database;
using MR.Interface;
using Plugin.LocalNotification;

namespace MR
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IRepository, SQLiteRepository>();
            builder.Services.AddSingleton<SystemClock>();
            builder.Services.AddSingleton<ReminderScheduler>();


     

            return builder.Build();
        }
    }
}
