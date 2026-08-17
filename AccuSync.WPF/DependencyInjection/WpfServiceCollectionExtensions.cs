// --------------------------------------------------------------------------------
// <copyright file="WpfServiceCollectionExtensions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.IO;
using System.Windows;
using AccuSync.Adapters.DataParser.DependencyInjection;
using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Services;
using AccuSync.EF.DependencyInjection;
using AccuSync.Presentation.ViewModels;
using AccuSync.WPF.Views.Dashboard;
using AccuSync.WPF.Views.Login;
using AccuSync.WPF.Views.Splash;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccuSync.WPF.DependencyInjection
{
    /// <summary>
    /// Registers every service, view model, and window the app's DI container needs.
    /// Kept out of App.xaml.cs so the WPF entry point stays a thin bootstrapper and this
    /// composition-root wiring can move independently if the app's startup mechanism
    /// ever changes — same "one extension method per project" pattern as
    /// <c>AddSqlitePersistence</c> and <c>AddDataParserServices</c>.
    /// </summary>
    public static class WpfServiceCollectionExtensions
    {
        private const string AppSettingsFileName = "appsettings.json";

        private const string DatabasePathConfigKey = "Persistence:DatabasePath";
        private const string DataProtectionKeysPathConfigKey = "Persistence:DataProtectionKeysPath";

        private const string DefaultDatabaseFolderVendor = "Natus";
        private const string DefaultDatabaseFolderProduct = "AccuSync";
        private const string DefaultDatabaseFileName = "SettingsDatabase.db";
        private const string DataProtectionKeysFolderName = "DataProtectionKeys";

        private const string DatabaseFolderErrorMessageFormat = "AccuSync could not prepare the database folder:\n{0}\n\n{1}\n\nThe application cannot start.";
        private const string DatabaseFolderErrorCaption = "AccuSync - Startup Error";

        /// <summary>Registers configuration-driven persistence plus every service, view model, and window the app needs.</summary>
        public static IServiceCollection AddWpfServices(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(AppSettingsFileName, optional: true)
                .Build();

            string databasePath = ResolveDatabasePath(configuration);

            // Persistence — provider selection happens right here: swapping database
            // engines later means calling a different sibling adapter project's
            // equivalent method instead of AddSqlitePersistence.
            services.AddSqlitePersistence(databasePath);

            // File-format exchange — import parsing, QR generation. Same "provider
            // selection happens here" pattern as AddSqlitePersistence above.
            services.AddDataParserServices();

            // Data Protection key ring lives alongside the database so it travels with
            // it if the DB file is ever backed up/restored onto another machine.
            // Standalone use only; no web server/hosting involved.
            string keysPath = ResolveDataProtectionKeysPath(configuration, databasePath);
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

            return services;
        }

        /// <summary>
        /// Reads Persistence:DatabasePath from appsettings.json. Falls back to the
        /// existing %ProgramData%\Natus\AccuSync\SettingsDatabase.db location when the
        /// setting is absent or blank, so the app works out of the box with zero config.
        /// </summary>
        private static string ResolveDatabasePath(IConfiguration configuration)
        {
            string configuredPath = configuration[DatabasePathConfigKey];

            string databasePath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    DefaultDatabaseFolderVendor, DefaultDatabaseFolderProduct, DefaultDatabaseFileName)
                : configuredPath;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                // Invalid Persistence:DatabasePath (bad characters, inaccessible drive, no
                // permission, etc.) or a filesystem failure — the app cannot run without a
                // usable database location, so fail clearly here instead of continuing into
                // an unhandled exception once EF Core tries to open the file.
                MessageBox.Show(
                    string.Format(DatabaseFolderErrorMessageFormat, databasePath, ex.Message),
                    DatabaseFolderErrorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            return databasePath;
        }

        /// <summary>
        /// Reads Persistence:DataProtectionKeysPath from appsettings.json. Falls back to a
        /// "DataProtectionKeys" folder alongside the database when the setting is absent
        /// or blank, so the key ring travels with the database if it's ever backed up or
        /// restored onto another machine.
        /// </summary>
        private static string ResolveDataProtectionKeysPath(IConfiguration configuration, string databasePath)
        {
            string configuredPath = configuration[DataProtectionKeysPathConfigKey];

            return string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(Path.GetDirectoryName(databasePath), DataProtectionKeysFolderName)
                : configuredPath;
        }
    }
}
