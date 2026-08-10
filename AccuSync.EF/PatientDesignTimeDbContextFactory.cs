// --------------------------------------------------------------------------------
// <copyright file="PatientDesignTimeDbContextFactory.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccuSync.EF
{
    /// <summary>
    /// Lets `dotnet ef migrations add`/`dotnet ef database update` construct a
    /// <see cref="PatientDbContext"/> from the command line, without running the full app
    /// or its DI container. See <see cref="SqliteDesignTimeDbContextFactory"/> for the
    /// equivalent on SettingsDbContext — the connection string here is only ever used at
    /// migration-design time.
    /// </summary>
    public class PatientDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PatientDbContext>
    {
        public PatientDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PatientDbContext>();
            optionsBuilder.UseSqlite("Data Source=design-time-patient.db", b => b.MigrationsAssembly("AccuSync.EF"));
            return new PatientDbContext(optionsBuilder.Options);
        }
    }
}
