using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Infraestructure.Data.MongoDB;
using MongoDB.Driver;

namespace FootballAPI.Infraestructure.Repositories.MongoDB
{
     /// <summary>
    /// MongoDB implementation of player repository
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Specialized Repository
    /// Implements domain-specific queries optimized for MongoDB.
    /// </remarks>
    public class MongoPlayerRepository : MongoRepository<Player>, IPlayerRepository
    {
        public MongoPlayerRepository(MongoDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Player>> GetTopCardHoldersAsync(bool yellowCard, int count)
        {
            if (yellowCard)
            {
                return await _collection.Find(p => p.YellowCard > 0)
                    .SortByDescending(p => p.YellowCard)
                    .Limit(count)
                    .ToListAsync();
            }
            else
            {
                return await _collection.Find(p => p.RedCard > 0)
                    .SortByDescending(p => p.RedCard)
                    .Limit(count)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Player>> GetTopByMinutesPlayedAsync(int count)
        {
            return await _collection.Find(p => p.MinutesPlayed > 0)
                .SortByDescending(p => p.MinutesPlayed)
                .Limit(count)
                .ToListAsync();
        }
    }
}