using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Infraestructure.Data.SQLServer;
using Microsoft.EntityFrameworkCore;

namespace FootballAPI.Infraestructure.Repositories.SqlServer
{
    /// <summary>
    /// SQL Server implementation of manager repository
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Specialized Repository
    /// Extends the generic repository with manager-specific operations.
    /// </remarks>
    public class SqlServerManagerRepository : SqlServerRepository<Manager>, IManagerRepository
    {
        public SqlServerManagerRepository(SqlServerDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Manager>> GetTopYellowCardHoldersAsync(int count)
        {
            return await _dbSet
                .Where(m => m.YellowCard > 0)
                .OrderByDescending(m => m.YellowCard)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Manager>> GetTopRedCardHoldersAsync(int count)
        {
            return await _dbSet
                .Where(m => m.RedCard > 0)
                .OrderByDescending(m => m.RedCard)
                .Take(count)
                .ToListAsync();
        }
    }
}