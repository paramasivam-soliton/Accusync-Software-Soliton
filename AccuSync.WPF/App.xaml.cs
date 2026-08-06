using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Application.Helpers;
using AccuSync.Application.Services.Authentication;
using AccuSync.Presentation.ViewModels;
using AccuSync.Adapters.DataParser.DependencyInjection;
using AccuSync.EF.DependencyInjection;
using AccuSync.WPF.Views.Dashboard;
using AccuSync.WPF.Views.Splash;
using AccuSync.WPF.Views.Login;

namespace AccuSync.WPF
{
    public partial class App : System.Windows.Application
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
            string databasePath = ResolveDatabasePath();

            // Persistence — provider selection happens right here: swapping database
            // engines later means calling a different sibling adapter project's
            // equivalent method instead of AddSqlitePersistence.
            services.AddSqlitePersistence(databasePath);

            // File-format exchange — import parsing, QR generation. Same "provider
            // selection happens here" pattern as AddSqlitePersistence above.
            services.AddDataParserServices();

            // Data Protection key ring lives alongside the database so it travels with
            // it if the DB file is ever backed up/restored onto another machine — see
            // LOGIN_EPIC_SPEC.md §2.2. Standalone use only; no web server/hosting involved.
            string keysPath = Path.Combine(Path.GetDirectoryName(databasePath), "DataProtectionKeys");
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            // Services
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
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

        /// <summary>
        /// Reads Persistence:DatabasePath from appsettings.json. Falls back to the
        /// existing %ProgramData%\Natus\AccuSync\SettingsDatabase.db location when the
        /// setting is absent or blank, so the app works out of the box with zero config.
        /// </summary>
        private static string ResolveDatabasePath()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            string configuredPath = configuration["Persistence:DatabasePath"];

            string databasePath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Natus", "AccuSync", "SettingsDatabase.db")
                : configuredPath;

            Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            return databasePath;
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
        /// Disposes the DI container, which closes the EF Core DbContext's SQLite
        /// connection cleanly. Without this, the connection stays open for the whole
        /// process lifetime and relies on the OS to release the file handle on process
        /// exit — usually fine, but leaves a window where a fast relaunch (e.g. restarting
        /// under a debugger) can find the file still locked by the previous process.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
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