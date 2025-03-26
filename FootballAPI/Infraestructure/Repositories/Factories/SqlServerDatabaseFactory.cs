using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;
using FootballAPI.Infraestructure.Repositories.SqlServer;

namespace FootballAPI.Infraestructure.Repositories.Factories
{
     public class SqlServerDatabaseFactory : IDatabaseFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerDatabaseFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRepository<T> CreateRepository<T>() where T : class, IEntity
        {
            // Try to get the concrete type first
            var repository = _serviceProvider.GetService<SqlServerRepository<T>>();
            if (repository != null)
                return repository;

            // If not found, get the generic repository
            return _serviceProvider.GetRequiredService<IRepository<T>>();
        }

        public IPlayerRepository CreatePlayerRepository() => 
            _serviceProvider.GetRequiredService<SqlServerPlayerRepository>();

        public IManagerRepository CreateManagerRepository() => 
            _serviceProvider.GetRequiredService<SqlServerManagerRepository>();

        public IUnitOfWork CreateUnitOfWork() => 
            _serviceProvider.GetRequiredService<SqlServerUnitOfWork>();
    }
}