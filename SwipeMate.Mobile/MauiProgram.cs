using Microsoft.Extensions.Logging;
#if ANDROID
using Android.Content.Res;
using Android.Widget;
using Microsoft.Maui.Handlers;
#endif
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

#if ANDROID
        ApplySwipeMateNativeColors();
#endif

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

#if ANDROID
    private static void ApplySwipeMateNativeColors()
    {
        var textColor = Android.Graphics.Color.Rgb(17, 24, 39);
        var hintColor = Android.Graphics.Color.Rgb(107, 114, 128);
        var accentColor = Android.Graphics.Color.Rgb(109, 40, 217);
        var trackColor = Android.Graphics.Color.Rgb(209, 213, 219);

        EntryHandler.Mapper.AppendToMapping("SwipeMateNativeColors", (handler, view) =>
        {
            handler.PlatformView.SetTextColor(textColor);
            handler.PlatformView.SetHintTextColor(hintColor);
            handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(accentColor);
        });

        EditorHandler.Mapper.AppendToMapping("SwipeMateNativeColors", (handler, view) =>
        {
            handler.PlatformView.SetTextColor(textColor);
            handler.PlatformView.SetHintTextColor(hintColor);
            handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(accentColor);
        });

        PickerHandler.Mapper.AppendToMapping("SwipeMateNativeColors", (handler, view) =>
        {
            handler.PlatformView.SetTextColor(textColor);
            handler.PlatformView.SetHintTextColor(hintColor);
            handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(accentColor);
        });

        CheckBoxHandler.Mapper.AppendToMapping("SwipeMateNativeColors", (handler, view) =>
        {
            handler.PlatformView.ButtonTintList = ColorStateList.ValueOf(accentColor);
        });

        SliderHandler.Mapper.AppendToMapping("SwipeMateNativeColors", (handler, view) =>
        {
            handler.PlatformView.ProgressTintList = ColorStateList.ValueOf(accentColor);
            handler.PlatformView.ThumbTintList = ColorStateList.ValueOf(accentColor);
            handler.PlatformView.ProgressBackgroundTintList = ColorStateList.ValueOf(trackColor);
        });
    }
#endif
}
