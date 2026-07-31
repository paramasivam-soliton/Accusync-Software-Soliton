// --------------------------------------------------------------------------------
// <copyright file="SqliteDesignTimeDbContextFactory.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccuSync.SQLite
{
    /// <summary>
    /// Lets `dotnet ef migrations add`/`dotnet ef database update` construct a
    /// <see cref="SettingsDbContext"/> from the command line, without running the
    /// full app or its DI container. The connection string here is only ever used
    /// at migration-design time — the real app always supplies its own path through
    /// <see cref="DependencyInjection.SqliteServiceCollectionExtensions.AddSqlitePersistence"/>.
    /// </summary>
    public class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SettingsDbContext>
    {
        public SettingsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SettingsDbContext>();
            optionsBuilder.UseSqlite("Data Source=design-time.db", b => b.MigrationsAssembly("AccuSync.SQLite"));
            return new SettingsDbContext(optionsBuilder.Options);
        }
    }
}
