// --------------------------------------------------------------------------------
// <copyright file="PatientDbContext.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using AccuSync.EF.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF.Contexts
{
    /// <summary>
    /// Patient/test data database (PatientDatabase.db). The context itself doesn't know which
    /// provider is active — that's configured by whichever DbContextOptions are passed in
    /// (see <see cref="DependencyInjection.SqliteServiceCollectionExtensions"/>).
    /// </summary>
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<PatientContact> PatientContacts => Set<PatientContact>();
        public DbSet<TestSession> TestSessions => Set<TestSession>();
        public DbSet<TestRecord> TestRecords => Set<TestRecord>();
        public DbSet<PatientRiskFactorValue> PatientRiskFactorValues => Set<PatientRiskFactorValue>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PatientConfiguration());
            modelBuilder.ApplyConfiguration(new PatientContactConfiguration());
            modelBuilder.ApplyConfiguration(new TestSessionConfiguration());
            modelBuilder.ApplyConfiguration(new TestRecordConfiguration());
            modelBuilder.ApplyConfiguration(new PatientRiskFactorValueConfiguration());
        }
    }
}
