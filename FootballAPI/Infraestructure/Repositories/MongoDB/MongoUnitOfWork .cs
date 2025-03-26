using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Infraestructure.Data.MongoDB;
using FootballAPI.Infraestructure.Repositories.MongoDB;


namespace FootballAPI.Infrastructure.Repositories.MongoDB
{
    /// <summary>
    /// MongoDB implementation of Unit of Work
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Unit of Work
    /// Coordinates operations and transaction management across MongoDB repositories.
    /// </remarks>
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly MongoDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed;

        public MongoUnitOfWork(
            MongoDbContext context,
            IPlayerRepository playerRepository,
            IManagerRepository managerRepository)
        {
            _context = context;
            PlayerRepository = playerRepository;
            ManagerRepository = managerRepository;
            
            // Initialize repositories not injected through the constructor
            RefereeRepository = new MongoRepository<Referee>(_context);
            MatchRepository = new MongoRepository<Match>(_context);
        }

        public IPlayerRepository PlayerRepository { get; }
        public IManagerRepository ManagerRepository { get; }
        public IRepository<Referee> RefereeRepository { get; }
        public IRepository<Match> MatchRepository { get; }

        public IRepository<T> Repository<T>() where T : class, IEntity
        {
            var type = typeof(T);
            
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new MongoRepository<T>(_context);
            }
            
            return (IRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.RollbackTransactionAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}

