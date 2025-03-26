using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Core.Interfaces
{
    /// <summary>
    /// MongoDB-specific entity interface for document IDs
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Interface Segregation Principle (ISP)
    /// Separates MongoDB-specific concerns from general entity contract.
    /// PRINCIPLE: Single Responsibility Principle
    /// This interface has only one reason to change: MongoDB document ID requirements.
    /// </remarks>
    public interface IMongoEntity
    {
        string DocumentId { get; set; }
    }
}