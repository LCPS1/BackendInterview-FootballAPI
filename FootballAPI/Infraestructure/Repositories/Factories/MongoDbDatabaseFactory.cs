using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;
using FootballAPI.Infraestructure.Repositories.MongoDB;
using FootballAPI.Infrastructure.Repositories.MongoDB;

namespace FootballAPI.Infraestructure.Repositories.Factories
{
    public class MongoDbDatabaseFactory : IDatabaseFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MongoDbDatabaseFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRepository<T> CreateRepository<T>() where T : class, IEntity
        {
            // Try to get the concrete type first
            var repository = _serviceProvider.GetService<MongoRepository<T>>();
            if (repository != null)
                return repository;

            // If not found, get the generic repository
            return _serviceProvider.GetRequiredService<IRepository<T>>();
        }

        public IPlayerRepository CreatePlayerRepository() => 
            _serviceProvider.GetRequiredService<MongoPlayerRepository>();

        public IManagerRepository CreateManagerRepository() => 
            _serviceProvider.GetRequiredService<MongoManagerRepository>();

        public IUnitOfWork CreateUnitOfWork() => 
            _serviceProvider.GetRequiredService<MongoUnitOfWork>();
    }
}