// --------------------------------------------------------------------------------
// <copyright file="ProfileRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for profiles (SettingsDatabase.db). Profiles
    /// aren't secret, so — unlike <see cref="UserRepository"/> — nothing here is
    /// encrypted.
    /// </summary>
    public class ProfileRepository : IProfileRepository
    {
        private readonly SettingsDbContext _context;

        /// <summary>Creates a repository backed by the given context.</summary>
        public ProfileRepository(SettingsDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns every stored profile.</summary>
        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            return await _context.Profiles.AsNoTracking().ToListAsync();
        }

        /// <summary>Looks up a single profile by id.</summary>
        public async Task<Profile> GetProfileByIdAsync(string profileId)
        {
            return await _context.Profiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == profileId);
        }

        /// <summary>Creates a new profile.</summary>
        public async Task<bool> CreateProfileAsync(Profile profile)
        {
            try
            {
                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Persists changes to an existing profile. Returns false if the profile no longer exists.</summary>
        public async Task<bool> UpdateProfileAsync(Profile profile)
        {
            try
            {
                var tracked = await _context.Profiles.FindAsync(profile.Id);
                if (tracked == null) return false;

                _context.Entry(tracked).CurrentValues.SetValues(profile);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
