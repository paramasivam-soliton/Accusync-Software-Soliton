// --------------------------------------------------------------------------------
// <copyright file="SqliteServiceCollectionExtensions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.EF;
using AccuSync.EF.Contexts;
using AccuSync.EF.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccuSync.EF.DependencyInjection
{
    /// <summary>
    /// Wires up SQLite as the active database provider. This is the one place in the
    /// solution — besides the executable's DI registration that calls it — that knows
    /// SQLite is in use. Swapping providers means adding an equivalent method here
    /// (and the matching provider package to this project); nothing else in the
    /// solution changes.
    /// </summary>
    public static class SqliteServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, string databasePath)
        {
            services.AddSingleton<TimestampInterceptor>();

            services.AddDbContext<SettingsDbContext>((provider, options) =>
            {
                // Default Timeout: seconds SQLite will retry before giving up on a locked
                // file, instead of failing immediately — covers the brief window where a
                // previous process instance hasn't fully released the file yet.
                options.UseSqlite($"Data Source={databasePath};Default Timeout=5", b => b.MigrationsAssembly("AccuSync.EF"));
                options.AddInterceptors(provider.GetRequiredService<TimestampInterceptor>());
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

            return services;
        }
    }
}
