using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FootballAPI.Core.Interfaces;
using FootballAPI.Infraestructure.Data.SQLServer;
using Microsoft.EntityFrameworkCore;

namespace FootballAPI.Infraestructure.Repositories.SqlServer
{

    // <summary>
    /// SQL Server implementation of generic repository
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Repository
    /// Abstracts the data access logic for SQL Server persistence.
    /// DESIGN PATTERN: Generic Repository
    /// Provides type-safe implementation for different entity types.
    /// </remarks>
    public class SqlServerRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly SqlServerDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public SqlServerRepository(SqlServerDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public IQueryable<T> GetAllWithInclude(params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            
            return query;
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludeAsync(Expression<Func<T, bool>> predicate, 
                                                             params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            var result = await _dbSet.AddAsync(entity);
            return result.Entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
    }
}