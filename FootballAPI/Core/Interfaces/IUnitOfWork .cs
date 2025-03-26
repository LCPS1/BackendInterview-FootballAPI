using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;


namespace FootballAPI.Core.Interfaces
{
    /// <summary>
    /// Coordinates operations across multiple repositories
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Unit of Work
    /// Maintains a list of objects affected by a business transaction and
    /// coordinates the writing out of changes and resolution of concurrency problems.
    /// 
    /// DESIGN PATTERN: Facade
    /// Provides a unified interface to a set of repositories.
    /// 
    /// PRINCIPLE: Single Responsibility Principle
    /// This interface has only one reason to change: transaction coordination.
    /// 
    /// PRINCIPLE: Interface Segregation Principle
    /// Clients get exactly the repository access they need.
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        // Repository access properties
        IRepository<T> Repository<T>() where T : class, IEntity;
        IPlayerRepository PlayerRepository { get; }
        IManagerRepository ManagerRepository { get; }
        IRepository<Referee> RefereeRepository { get; }
        IRepository<Match> MatchRepository { get; }
        
        // Transaction operations
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}