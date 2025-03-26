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
    /// MongoDB implementation of manager repository
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Specialized Repository
    /// Implements domain-specific queries optimized for MongoDB.
    /// </remarks>
    public class MongoManagerRepository : MongoRepository<Manager>, IManagerRepository
    {
        public MongoManagerRepository(MongoDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Manager>> GetTopYellowCardHoldersAsync(int count)
        {
            return await _collection.Find(m => m.YellowCard > 0)
                .SortByDescending(m => m.YellowCard)
                .Limit(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Manager>> GetTopRedCardHoldersAsync(int count)
        {
            return await _collection.Find(m => m.RedCard > 0)
                .SortByDescending(m => m.RedCard)
                .Limit(count)
                .ToListAsync();
        }
    }
}