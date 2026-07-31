// --------------------------------------------------------------------------------
// <copyright file="UnitOfWork.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using AccuSync.Application.Abstractions.Repositories;
using AccuSync.Persistence.Contexts;

namespace AccuSync.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SettingsDbContext _context;

        public UnitOfWork(SettingsDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
