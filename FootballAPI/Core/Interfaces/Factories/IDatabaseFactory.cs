using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Core.Interfaces.Factories
{
    public interface IDatabaseFactory
    {
        IUnitOfWork CreateUnitOfWork();
        IRepository<T> CreateRepository<T>() where T : class, IEntity;
        IPlayerRepository CreatePlayerRepository();
        IManagerRepository CreateManagerRepository();
    }
}