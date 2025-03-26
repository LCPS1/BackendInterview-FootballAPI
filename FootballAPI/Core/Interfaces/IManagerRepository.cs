using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;

namespace FootballAPI.Core.Interfaces
{
    public interface IManagerRepository : IRepository<Manager>
    {        
        Task<IEnumerable<Manager>> GetTopYellowCardHoldersAsync(int count);
        Task<IEnumerable<Manager>> GetTopRedCardHoldersAsync(int count);
    }
}