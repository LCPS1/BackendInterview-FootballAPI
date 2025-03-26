using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;

namespace FootballAPI.Core.Interfaces
{
    public interface IPlayerRepository : IRepository<Player>
    {   
        Task<IEnumerable<Player>> GetTopCardHoldersAsync(bool yellowCard, int count);
        Task<IEnumerable<Player>> GetTopByMinutesPlayedAsync(int count);
    }
}