using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;

namespace FootballAPI.Infraestructure.Repositories.Factories
{
        /// <summary>
        /// Factory for creating database-specific repositories
        /// </summary>
        /// <remarks>
        /// DESIGN PATTERN: Abstract Factory
        /// Creates families of related objects (repositories) based on configuration.
        /// DESIGN PATTERN: Strategy
        /// Selects the appropriate database implementation at runtime.
        /// </remarks>
    public class RepositoryFactory : IRepositoryFactory
    {
    private readonly IDatabaseFactory _databaseFactory;

        public RepositoryFactory(
            IConfiguration configuration, 
            SqlServerDatabaseFactory sqlServerFactory,
            MongoDbDatabaseFactory mongoDbFactory)
        {
            var databaseType = configuration.GetValue<string>("DatabaseType") ?? "SqlServer";
            
            _databaseFactory = databaseType.ToLower() switch
            {
                "mongodb" => mongoDbFactory,
                _ => sqlServerFactory
            };
        }
        public IRepository<T> CreateRepository<T>() where T : class, IEntity => 
            _databaseFactory.CreateRepository<T>();

        public IPlayerRepository CreatePlayerRepository() => 
            _databaseFactory.CreatePlayerRepository();

        public IManagerRepository CreateManagerRepository() => 
            _databaseFactory.CreateManagerRepository();

        public IUnitOfWork CreateUnitOfWork() => 
            _databaseFactory.CreateUnitOfWork();
    }

}