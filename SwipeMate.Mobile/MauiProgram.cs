using Microsoft.Extensions.Logging;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile
{
    public static class MauiProgram
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
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(sp =>
            {
                var http = new HttpClient
                {
                    BaseAddress = new Uri("https://10.0.2.2:7193") // Android emulator към localhost
                };
                return new ApiClient(http);
            });

            builder.Services.AddSingleton<AuthService>();


            return builder.Build();
        }
    }
}
