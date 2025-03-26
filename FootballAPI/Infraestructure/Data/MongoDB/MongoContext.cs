using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using FootballAPI.Core.Entities;

namespace FootballAPI.Infraestructure.Data.MongoDB
{
      /// <summary>
    /// MongoDB database context
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Facade
    /// Provides a simplified interface to the MongoDB driver API.
    /// </remarks>
    public class MongoDbContext : IDisposable
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;
        private IClientSessionHandle _session;
        private bool _disposed;

        public MongoDbContext(IConfiguration configuration)
        {
            // Register MongoDB conventions
            RegisterConventions();
            
            // Configure entity class mappings
            ConfigureClassMaps();

            // Get connection details from configuration
            var connectionString = configuration.GetConnectionString("MongoConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "mongodb://localhost:27017";
            }
            
            var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName") ?? "FootballAPI";
            
            // Create MongoDB client and get database
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Registers MongoDB conventions for serialization
        /// </summary>
        private static void RegisterConventions()
        {
            // Ignore null values
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfNullConvention(true)
            };
            
            ConventionRegistry.Register("FootballAPIConventions", conventionPack, t => true);
        }

        /// <summary>
        /// Configure MongoDB class mappings
        /// </summary>
        private static void ConfigureClassMaps()
        {
            // Only register if not already registered
            if (!BsonClassMap.IsClassMapRegistered(typeof(Player)))
            {
                BsonClassMap.RegisterClassMap<Player>(cm => {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            
            if (!BsonClassMap.IsClassMapRegistered(typeof(Manager)))
            {
                BsonClassMap.RegisterClassMap<Manager>(cm => {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            
            if (!BsonClassMap.IsClassMapRegistered(typeof(Match)))
            {
                BsonClassMap.RegisterClassMap<Match>(cm => {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            
            if (!BsonClassMap.IsClassMapRegistered(typeof(Referee)))
            {
                BsonClassMap.RegisterClassMap<Referee>(cm => {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }

        /// <summary>
        /// Get a MongoDB collection for the specified entity type
        /// </summary>
        public IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).Name + "s");
        }

        /// <summary>
        /// Save changes to MongoDB (no-op since MongoDB saves immediately)
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            // MongoDB operations are immediate, no explicit save needed
            return await Task.FromResult(1);
        }

        /// <summary>
        /// Begin a new transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            try
            {
                _session = await _client.StartSessionAsync();
                _session.StartTransaction();
            }
            catch (NotSupportedException ex)
            {
                // Log that transactions require a replica set
                Console.WriteLine($"MongoDB transactions not supported: {ex.Message}");
                // Create session without transaction support
                _session = await _client.StartSessionAsync();
            }
        }

        /// <summary>
        /// Get the current MongoDB session
        /// </summary>
        public IClientSessionHandle GetSession()
        {
            return _session;
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_session != null && _session.IsInTransaction)
                {
                    await _session.CommitTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction commit error: {ex.Message}");
                await _session.AbortTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_session != null && _session.IsInTransaction)
                {
                    await _session.AbortTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction rollback error: {ex.Message}");
                throw;
            }
        }
        public IMongoDatabase GetDatabase()
        {
            return _database;
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
                    _session?.Dispose();
                    _session = null;
                }
                _disposed = true;
            }
        }

        public bool SupportsTransactions
        {
            get
            {
                try
                {
                    // Check if the current MongoDB deployment supports transactions
                    return _client.GetDatabase("admin")
                        .RunCommand<BsonDocument>(new BsonDocument("buildInfo", 1))
                        .GetValue("version")
                        .AsString
                        .Split('.')
                        .Select(int.Parse)
                        .ToArray() is [var major, _, _] && major >= 4;
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}