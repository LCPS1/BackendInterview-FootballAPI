using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace FootballAPI.Infraestructure.Data.SQLServer
{
    /// <summary>
    /// SQL Server database context using Entity Framework Core
    /// </summary>
    /// <remarks>
    /// DESIGN PATTERN: Unit of Work
    /// The DbContext acts as a Unit of Work, tracking changes across multiple entities.
    /// </remarks>
    public class SqlServerDbContext : DbContext
    {
        private IDbContextTransaction _transaction;

        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Manager> Managers { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Referee> Referees { get; set; }
        public DbSet<Match> Matches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore MongoDB-specific properties
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute)));
                
                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.ClrType).Ignore(property.Name);
                }
            }

            // Configure Match entity
            modelBuilder.Entity<Match>(entity =>
            {
                // Configure Match-Manager relationships 
                entity.HasOne(m => m.HouseManager)
                    .WithMany()
                    .HasForeignKey("HouseManagerId")
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(m => m.AwayManager)
                    .WithMany()
                    .HasForeignKey("AwayManagerId")
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure Match-Referee relationship
                entity.HasOne(m => m.Referee)
                    .WithMany()
                    .HasForeignKey("RefereeId");

                // Configure Match-Player relationships
                entity.HasMany(m => m.HousePlayers)
                    .WithOne()
                    .HasForeignKey("HouseMatchId")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.AwayPlayers)
                    .WithOne()
                    .HasForeignKey("AwayMatchId")
                    .OnDelete(DeleteBehavior.Restrict);
                    
                // Configure ScheduledStart property
                entity.Property(m => m.ScheduledStart)
                    .IsRequired();
                    
                // Configure Status property
                entity.Property(m => m.Status)
                    .IsRequired()
                    .HasDefaultValue(MatchStatus.Scheduled);
            });

            // Configure Player entity
            modelBuilder.Entity<Player>(entity =>
            {
                entity.Property(p => p.Id)
                    .ValueGeneratedOnAdd();
                    
                entity.Property(p => p.Name)
                    .IsRequired(false)
                    .HasMaxLength(100);
                    
                entity.Property(p => p.YellowCard)
                    .HasDefaultValue(0);
                    
                entity.Property(p => p.RedCard)
                    .HasDefaultValue(0);
                    
                entity.Property(p => p.MinutesPlayed)
                    .HasDefaultValue(0);
                    
                // Make HouseMatchId and AwayMatchId nullable as players can be unassigned
                entity.Property(p => p.HouseMatchId)
                    .IsRequired(false);
                    
                entity.Property(p => p.AwayMatchId)
                    .IsRequired(false);
            });

            // Configure Manager entity
            modelBuilder.Entity<Manager>(entity =>
            {
                entity.Property(m => m.Id)
                    .ValueGeneratedOnAdd();
                    
                entity.Property(m => m.Name)
                    .IsRequired(false)
                    .HasMaxLength(100);
                    
                entity.Property(m => m.YellowCard)
                    .HasDefaultValue(0);
                    
                entity.Property(m => m.RedCard)
                    .HasDefaultValue(0);
            });

            // Configure Referee entity
            modelBuilder.Entity<Referee>(entity =>
            {
                entity.Property(r => r.Id)
                    .ValueGeneratedOnAdd();
                    
                entity.Property(r => r.Name)
                    .IsRequired(false)
                    .HasMaxLength(100);
                    
                entity.Property(r => r.MinutesPlayed)
                    .HasDefaultValue(0);
            });
        }

        /// <summary>
        /// Begin a new transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}

