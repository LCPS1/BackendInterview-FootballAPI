using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;
using FootballAPI.Infraestructure.Data.SQLServer;

namespace FootballAPI.Infraestructure.Repositories.SqlServer
{
    /// <summary>
    /// SQL Server implementation of Unit of Work
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Unit of Work
    /// Coordinates operations across multiple repositories.
    /// </remarks>
 public class SqlServerUnitOfWork : IUnitOfWork
    {
        private readonly SqlServerDbContext _context;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed;

        public SqlServerUnitOfWork(
            SqlServerDbContext context,
            IRepositoryFactory repositoryFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));

            // Initialize repositories using factory
            PlayerRepository = _repositoryFactory.CreatePlayerRepository();
            ManagerRepository = _repositoryFactory.CreateManagerRepository();
            RefereeRepository = _repositoryFactory.CreateRepository<Referee>();
            MatchRepository = _repositoryFactory.CreateRepository<Match>();
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
                _repositories[type] = _repositoryFactory.CreateRepository<T>();
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