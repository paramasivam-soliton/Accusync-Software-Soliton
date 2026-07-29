using System.Diagnostics;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AccuSync.Helpers;
using AccuSync.Services;
using AccuSync.ViewModels;
using AccuSync.WPF.Views;
using AccuSync.WPF.Views.Splash;
using AccuSync.WPF.Views.Login;

namespace AccuSync.WPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            // ViewModels
            services.AddTransient<SplashViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ChangePasswordViewModel>();

            // Views
            services.AddTransient<SplashWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<ChangePasswordWindow>();
            services.AddTransient<AdminDashboardWindow>();
            services.AddTransient<ScreenerDashboardWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Log dev mode status on startup
            if (DevModeConfig.IsAnyDevMode)
            {
                Debug.WriteLine($"[AccuSync] Dev mode active:{DevModeConfig.GetDevModeLabel()}");
                if (DevModeConfig.SkipLogin)
                {
                    var (username, role) = DevModeConfig.GetDevUser();
                    Debug.WriteLine($"[AccuSync]   Login bypass → User: {username}, Role: {role}");
                }
                if (DevModeConfig.SkipDashboard)
                {
                    Debug.WriteLine($"[AccuSync]   Dashboard bypass → Target: {DevModeConfig.GetTargetScreen()}");
                }
            }

            // Always start with splash (it handles DB init)
            // The splash will call NavigateAfterSplash() when done
            var splashWindow = _serviceProvider.GetRequiredService<SplashWindow>();
            splashWindow.Show();
        }

        /// <summary>
        /// Called by SplashWindow after initialization is complete.
        /// Handles the dev mode bypass routing.
        /// </summary>
        public static void NavigateAfterSplash(Window splashWindow)
        {
            if (DevModeConfig.SkipLogin)
            {
                // Bypass login → go to dashboard or target screen
                var (username, role) = DevModeConfig.GetDevUser();

                if (DevModeConfig.SkipDashboard)
                {
                    // Bypass both → go directly to target screen
                    NavigateToTargetScreen(DevModeConfig.GetTargetScreen(), username, role);
                }
                else
                {
                    // Bypass login only → show appropriate dashboard
                    NavigateToDashboard(username, role);
                }
            }
            else
            {
                // Normal flow → show login
                var loginWindow = GetService<LoginWindow>();
                loginWindow.Show();
            }

            splashWindow.Close();
        }

        /// <summary>
        /// Called after successful login (from LoginWindow) or when skipping login.
        /// </summary>
        public static void NavigateAfterLogin(Window currentWindow, string username, string role)
        {
            if (DevModeConfig.SkipDashboard)
            {
                NavigateToTargetScreen(DevModeConfig.GetTargetScreen(), username, role);
            }
            else
            {
                NavigateToDashboard(username, role);
            }

            currentWindow.Close();
        }

        private static void NavigateToDashboard(string username, string role)
        {
            if (role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                var dashboard = GetService<AdminDashboardWindow>();
                dashboard.SetCurrentUser(username);
                dashboard.Show();
            }
            else
            {
                var dashboard = GetService<ScreenerDashboardWindow>();
                // TODO: Add SetCurrentUser to ScreenerDashboardWindow when implemented
                dashboard.Show();
            }
        }

        private static void NavigateToTargetScreen(string screenName, string username, string role)
        {
            // All views are hosted inside the dashboard window.
            // We create the dashboard, set the user, then navigate to the target view.
            AdminDashboardWindow dashboard;

            if (role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                dashboard = GetService<AdminDashboardWindow>();
            }
            else
            {
                // For non-admin roles, open ScreenerDashboard instead
                // (ScreenerDashboard would need similar NavigateToView support)
                var screenerDash = GetService<ScreenerDashboardWindow>();
                screenerDash.Show();
                return;
            }

            dashboard.SetCurrentUser(username);
            dashboard.Show();

            // Navigate to the target view within the dashboard
            dashboard.NavigateToView(screenName);

            Debug.WriteLine($"[AccuSync] Dev mode → navigated to '{screenName}' as {username} ({role})");
        }

        public static T GetService<T>()
        {
            return ((App)Current)._serviceProvider.GetRequiredService<T>();
        }
    }
}