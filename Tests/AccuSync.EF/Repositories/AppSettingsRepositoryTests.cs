// --------------------------------------------------------------------------------
// <copyright file="AppSettingsRepositoryTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace AccuSync.EF.Tests.Repositories
{
    /// <summary>
    /// Mocks IDbContextFactory/DbContext/DbSet (Moq + MockQueryable.Moq for the async LINQ
    /// surface), backed by a plain in-memory list standing in for the AppSettings table.
    /// "Exactly one row" here is enforced by AppSettingsRepository's own find-or-create
    /// logic, not a database constraint, so there's no real-database counterpart needed
    /// for this repository (contrast <see cref="UserRepositoryConstraintTests"/>).
    /// </summary>
    public class AppSettingsRepositoryTests
    {
        private readonly List<AppSettings> _appSettings;
        private readonly AppSettingsRepository _appSettingsRepository;

        public AppSettingsRepositoryTests()
        {
            _appSettings = new List<AppSettings>();

            var appSettingsDbSetMock = _appSettings.BuildMockDbSet();
            appSettingsDbSetMock
                .Setup(d => d.Add(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(s => _appSettings.Add(s));

            var contextMock = new Mock<SettingsDbContext>(new DbContextOptionsBuilder<SettingsDbContext>().Options);
            contextMock.Setup(c => c.AppSettings).Returns(appSettingsDbSetMock.Object);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var contextFactoryMock = new Mock<IDbContextFactory<SettingsDbContext>>();
            contextFactoryMock
                .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            _appSettingsRepository = new AppSettingsRepository(contextFactoryMock.Object);
        }

        [Fact]
        public async Task GetLockoutDurationMinutesAsync_NoSettingsRowExistsYet_ReturnsTheDefaultOfFifteenMinutes()
        {
            int minutes = await _appSettingsRepository.GetLockoutDurationMinutesAsync();

            Assert.Equal(15, minutes);
        }

        [Fact]
        public async Task SetLockoutDurationMinutesAsync_NoSettingsRowExistsYet_CreatesItAndLaterReadsReturnTheNewValue()
        {
            bool success = await _appSettingsRepository.SetLockoutDurationMinutesAsync(30);

            Assert.True(success);
            Assert.Equal(30, await _appSettingsRepository.GetLockoutDurationMinutesAsync());
        }

        [Fact]
        public async Task SetLockoutDurationMinutesAsync_ExistingSettingsRowSetAgain_OverwritesRatherThanCreatingASecondRow()
        {
            await _appSettingsRepository.GetLockoutDurationMinutesAsync();

            await _appSettingsRepository.SetLockoutDurationMinutesAsync(45);

            Assert.Single(_appSettings);
            Assert.Equal(45, await _appSettingsRepository.GetLockoutDurationMinutesAsync());
        }
    }
}
