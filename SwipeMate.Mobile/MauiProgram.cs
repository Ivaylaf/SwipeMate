using Microsoft.Extensions.Logging;
using SwipeMate.Mobile.Pages;
using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile;

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
            var http = new HttpClient();

            return new ApiClient(http);
        });

        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<SwipeMateApiService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<FriendsPage>();
        builder.Services.AddTransient<CreateSessionPage>();
        builder.Services.AddTransient<SwipePage>();
        builder.Services.AddTransient<MatchPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ActiveSessionsPage>();
        builder.Services.AddTransient<AdminPage>();

        return builder.Build();
    }
}
