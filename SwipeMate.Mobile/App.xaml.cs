using SwipeMate.Mobile.Services;

namespace SwipeMate.Mobile;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly AppState _appState;

    public App(AppShell shell, AppState appState)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        _shell = shell;
        _appState = appState;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_shell);
        _ = InitializeAsync();
        return window;
    }

    private async Task InitializeAsync()
    {
        await _appState.InitializeAsync();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_appState.IsAuthenticated)
            {
                await _shell.GoToAsync("//Home");
            }
            else
            {
                await _shell.GoToAsync("//Login");
            }
        });
    }
}


