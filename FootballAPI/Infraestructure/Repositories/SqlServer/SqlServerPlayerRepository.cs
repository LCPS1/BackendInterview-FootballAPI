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
    /// SQL Server implementation of player repository
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Specialized Repository
    /// Extends the generic repository with player-specific operations.
    /// </remarks>
    public class SqlServerPlayerRepository : SqlServerRepository<Player>, IPlayerRepository
    {
        public SqlServerPlayerRepository(SqlServerDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Player>> GetTopCardHoldersAsync(bool yellowCard, int count)
        {
            if (yellowCard)
            {
                return await _dbSet
                    .Where(p => p.YellowCard > 0)
                    .OrderByDescending(p => p.YellowCard)
                    .Take(count)
                    .ToListAsync();
            }
            else
            {
                return await _dbSet
                    .Where(p => p.RedCard > 0)
                    .OrderByDescending(p => p.RedCard)
                    .Take(count)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Player>> GetTopByMinutesPlayedAsync(int count)
        {
            return await _dbSet
                .Where(p => p.MinutesPlayed > 0)
                .OrderByDescending(p => p.MinutesPlayed)
                .Take(count)
                .ToListAsync();
        }
    }
}