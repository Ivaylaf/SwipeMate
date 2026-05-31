using SwipeMate.Mobile.Pages;

namespace SwipeMate.Mobile;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _services;

    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;

        Items.Add(CreateRoot("Login", typeof(LoginPage)));
        Items.Add(CreateRoot("Register", typeof(RegisterPage)));
        Items.Add(CreateRoot("Home", typeof(HomePage)));

        Routing.RegisterRoute(nameof(FriendsPage), typeof(FriendsPage));
        Routing.RegisterRoute(nameof(CreateSessionPage), typeof(CreateSessionPage));
        Routing.RegisterRoute(nameof(SwipePage), typeof(SwipePage));
        Routing.RegisterRoute(nameof(MatchPage), typeof(MatchPage));
        Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(ActiveSessionsPage), typeof(ActiveSessionsPage));
        Routing.RegisterRoute(nameof(AdminPage), typeof(AdminPage));
    }

    private ShellContent CreateRoot(string route, Type pageType)
    {
        return new ShellContent
        {
            Route = route,
            ContentTemplate = new DataTemplate(() => (Page)_services.GetRequiredService(pageType))
        };
    }
}

