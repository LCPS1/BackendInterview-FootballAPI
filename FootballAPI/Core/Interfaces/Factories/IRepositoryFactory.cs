using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Core.Interfaces.Factories
{ 
    /// <summary>
    /// Factory for creating appropriate repositories at runtime
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Abstract Factory
    /// Creates families of related objects (repositories) without
    /// specifying their concrete classes.
    /// 
    /// DESIGN PATTERN: Factory Method
    /// Defines interface for creating objects, but lets subclasses decide
    /// which concrete classes to instantiate.
    /// 
    /// PRINCIPLE: Open/Closed Principle
    /// New database types can be added without modifying existing code.
    /// </remarks>
    public interface IRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class, IEntity;
        IPlayerRepository CreatePlayerRepository();
        IManagerRepository CreateManagerRepository();
        IUnitOfWork CreateUnitOfWork();
        
    }
}