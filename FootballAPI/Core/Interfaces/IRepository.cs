using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FootballAPI.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <remarks>
    /// DESIGN PATTERN: Repository Pattern
    /// Mediates between the domain and data mapping layers, acting like a collection.
    /// 
    /// DESIGN PATTERN: Command Query Separation (CQS)
    /// Separates methods that change state from those that don't.
    /// 
    /// PRINCIPLE: Interface Segregation Principle
    /// Defines a focused set of methods that clients need.
    /// 
    /// PRINCIPLE: Dependency Inversion Principle
    /// High-level modules (services) depend on this abstraction, not concrete implementations.
    /// </remarks>
    public interface IRepository<T> where T : class, IEntity
    {
        // Query operations (don't change state)
        IQueryable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Include operations for handling relationships
        IQueryable<T> GetAllWithInclude(params string[] includeProperties);
        Task<IEnumerable<T>> FindWithIncludeAsync(Expression<Func<T, bool>> predicate, params string[] includeProperties);
        
        // Command operations (change state)
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(T entity);
    }
}