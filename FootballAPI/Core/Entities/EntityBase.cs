using FootballAPI.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballAPI.Core.Entities
{
    /// <summary>
    /// Base class for all domain entities
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Template Method
    /// Provides base implementation for all entities.
    /// 
    /// DESIGN PATTERN: Adapter
    /// Adapts between SQL and MongoDB ID requirements.
    /// 
    /// PRINCIPLE: Don't Repeat Yourself (DRY)
    /// Centralizes common entity properties in one place.
    /// 
    /// NOTE: This design accommodates both SQL and MongoDB without leaking
    /// database concerns into domain entities. The NotMapped attribute
    /// ensures SqlServer won't try to map DocumentId to a database column.
    /// </remarks>
    public abstract class EntityBase : IEntity, IMongoEntity
    {
        /// <summary>
        /// Primary key for relational databases
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// MongoDB document identifier
        /// Ignored by Entity Framework for SQL Server
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [NotMapped] // Entity Framework will ignore this property
        public string DocumentId { get; set; }
    }
}