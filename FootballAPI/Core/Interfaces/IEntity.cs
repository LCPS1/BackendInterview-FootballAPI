using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Core.Interfaces
{
    public interface IEntity
    {
        /// <summary>
        /// Base interface for all domain entities.
        /// </summary>
        /// <remarks>
        /// DESIGN PATTERN: Marker Interface
        /// Provides a common type for all entities and enforces ID property.
        /// PRINCIPLE: Interface Segregation Principle
        /// Keeps the interface minimal, so clients only depend on what they need.
        /// </remarks>
        public interface IEntity
        {
            int Id { get; set; }
        }
    }
}