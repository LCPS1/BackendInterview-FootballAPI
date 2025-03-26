using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Infraestructure.Data.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;


namespace FootballAPI.Infraestructure.Repositories.MongoDB
{
   /// <summary>
    /// MongoDB implementation of generic repository
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Repository
    /// Abstracts the data access logic for MongoDB persistence.
    /// DESIGN PATTERN: Adapter
    /// Adapts the MongoDB driver operations to match the repository interface.
    /// </remarks>
    public class MongoRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly MongoDbContext _context;
        private readonly PropertyInfo _idProperty;

        public MongoRepository(MongoDbContext context)
        {
            _context = context;
            _collection = context.GetCollection<T>();
            _idProperty = typeof(T).GetProperty("Id");
        }

        public IQueryable<T> GetAll()
        {
            return _collection.AsQueryable();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public IQueryable<T> GetAllWithInclude(params string[] includeProperties)
        {
            // MongoDB doesn't need explicit includes due to document-based nature
            // But we could apply additional aggregation here if needed
            return _collection.AsQueryable();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            // Use string-based filter definition instead of lambda
            var filter = Builders<T>.Filter.Eq("Id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludeAsync(
            Expression<Func<T, bool>> predicate, 
            params string[] includeProperties)
        {
            // MongoDB handles document relationships differently
            // We would use aggregation pipeline here for complex lookups if needed
            return await _collection.Find(predicate).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            // Handle MongoDB document ID if entity implements IMongoEntity
            if (entity is IMongoEntity mongoEntity && string.IsNullOrEmpty(mongoEntity.DocumentId))
            {
                mongoEntity.DocumentId = ObjectId.GenerateNewId().ToString();
            }
            
            var session = _context.GetSession();
            if (session != null && session.IsInTransaction)
            {
                await _collection.InsertOneAsync(session, entity);
            }
            else
            {
                await _collection.InsertOneAsync(entity);
            }
            
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var session = _context.GetSession();
            
            // Get the entity ID without using the property directly
            var id = GetEntityId(entity);
            
            // Use string-based field name for ID
            var filter = Builders<T>.Filter.Eq("Id", id);
            
            if (session != null && session.IsInTransaction)
            {
                await _collection.ReplaceOneAsync(session, filter, entity);
            }
            else
            {
                await _collection.ReplaceOneAsync(filter, entity);
            }
            
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var session = _context.GetSession();
            
            // Use string-based field name for ID
            var filter = Builders<T>.Filter.Eq("Id", id);
            
            if (session != null && session.IsInTransaction)
            {
                await _collection.DeleteOneAsync(session, filter);
            }
            else
            {
                await _collection.DeleteOneAsync(filter);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            // Get the entity ID without using the property directly
            var id = GetEntityId(entity);
            
            // Use string-based field name for ID
            var filter = Builders<T>.Filter.Eq("Id", id);
            
            var session = _context.GetSession();
            if (session != null && session.IsInTransaction)
            {
                await _collection.DeleteOneAsync(session, filter);
            }
            else
            {
                await _collection.DeleteOneAsync(filter);
            }
        }

        // Helper method to get entity ID via reflection to avoid generic constraint issues
        private int GetEntityId(T entity)
        {
            // Use cached reflection to access the Id property
            return (int)_idProperty.GetValue(entity);
        }
    }
}