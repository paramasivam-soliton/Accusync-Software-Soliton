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
    /// Patient/test-data database (PatientDatabase.db), separate from
    /// <see cref="SettingsDbContext"/> — SQLite cannot enforce foreign keys across
    /// separate database files, so cross-database references (SiteId, AssignedUserId,
    /// MatchedDeviceId, MatchedProtocolId) are plain nullable ints validated at the
    /// repository layer. Mirrors <see cref="SettingsDbContext"/>'s shape exactly: no
    /// <c>OnConfiguring</c>, provider injected via DI.
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PatientConfiguration());
            modelBuilder.ApplyConfiguration(new PatientContactConfiguration());
            modelBuilder.ApplyConfiguration(new TestSessionConfiguration());
            modelBuilder.ApplyConfiguration(new TestRecordConfiguration());
        }
    }
}
