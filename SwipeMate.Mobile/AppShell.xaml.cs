using SwipeMate.Mobile.Pages;


namespace SwipeMate.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("Register", typeof(RegisterPage));
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Home", typeof(HomePage));
        }

    }
}
