using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballAPI.Infraestructure.Data.SQLServer
{
        public static class SqlServerDbInitializer
    {
        public static async Task Initialize(SqlServerDbContext context, IConfiguration configuration, ILogger logger)
        {
            try
            {
                await EnsureDatabaseExists(configuration, logger);

                // Ensure schema exists
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database schema ensured.");

                // Check if we need to seed
                if (await context.Players.AnyAsync())
                {
                    await EnsureMatchesHaveScheduledStart(context, logger);
                    logger.LogInformation("Existing data updated.");
                    return;
                }

                // Seed initial data
                logger.LogInformation("Starting database seeding...");

                var players = new List<Player>
                {
                    new() { Name = "Lionel" },
                    new() { Name = "Cristiano" },
                    new() { Name = "Iker" },
                    new() { Name = "Gerard" },
                    new() { Name = "Philippe" },
                    new() { Name = "Jordi" },
                    // Adding more players to have enough for matches
                    new() { Name = "Kylian" },
                    new() { Name = "Erling" },
                    new() { Name = "Kevin" },
                    new() { Name = "Virgil" },
                    new() { Name = "Robert" },
                    new() { Name = "Sadio" },
                    new() { Name = "Mohamed" },
                    new() { Name = "Trent" },
                    new() { Name = "Joshua" },
                    new() { Name = "Harry" },
                    new() { Name = "Marcus" },
                    new() { Name = "Mason" },
                    new() { Name = "Jadon" },
                    new() { Name = "Phil" },
                    new() { Name = "Bukayo" },
                    new() { Name = "Jude" }
                };
                await context.Players.AddRangeAsync(players);

                var managers = new List<Manager>
                {
                    new() { Name = "Alex" },
                    new() { Name = "Zidane" },
                    new() { Name = "Guardiola" }
                };
                await context.Managers.AddRangeAsync(managers);

                var referees = new List<Referee>
                {
                    new() { Name = "Pierluigi" },
                    new() { Name = "Howard" }
                };
                await context.Referees.AddRangeAsync(referees);
                await context.SaveChangesAsync();

                var matches = new List<Match>
                {
                    new Match
                    {
                        HouseManagerId = managers[0].Id,
                        AwayManagerId = managers[1].Id,
                        RefereeId = referees[0].Id,
                        HousePlayers = players.Take(11).ToList(),
                        AwayPlayers = players.Skip(11).Take(11).ToList(),
                        ScheduledStart = DateTime.UtcNow.AddMinutes(4),
                        Status = MatchStatus.Scheduled
                    },
                    new Match
                    {
                        HouseManagerId = managers[1].Id,
                        AwayManagerId = managers[2].Id,
                        RefereeId = referees[1].Id,
                        // This one has incorrect alignment (missing players)
                        HousePlayers = players.Take(5).ToList(),
                        AwayPlayers = players.Skip(11).Take(7).ToList(),
                        ScheduledStart = DateTime.UtcNow.AddMinutes(3),
                        Status = MatchStatus.Scheduled
                    }
                };
                await context.Matches.AddRangeAsync(matches);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization.");
                throw;
            }
        }

        private static async Task EnsureMatchesHaveScheduledStart(SqlServerDbContext context, ILogger logger)
        {
            try
            {
                var matches = await context.Matches.ToListAsync();
                var updated = false;

                foreach (var match in matches)
                {
                    if (match.ScheduledStart == default)
                    {
                        match.ScheduledStart = DateTime.UtcNow.AddMinutes(new Random().Next(2, 10));
                        updated = true;
                    }
                }

                if (updated)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Updated matches with ScheduledStart times");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating match scheduled times");
                throw;
            }
        }

        private static async Task EnsureDatabaseExists(IConfiguration configuration, ILogger logger)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var builder = new SqlConnectionStringBuilder(connectionString) 
            { 
                InitialCatalog = "master",
                TrustServerCertificate = true
            };
            var masterConnectionString = builder.ConnectionString;
            var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

            var maxRetryCount = 10;
            var retryInterval = TimeSpan.FromSeconds(5);

            // Add initial delay to allow SQL Server to fully start
            await Task.Delay(TimeSpan.FromSeconds(15));

            for (int i = 0; i < maxRetryCount; i++)
            {
                try
                {
                    logger.LogInformation($"Attempting connection to master database (attempt {i + 1} of {maxRetryCount})...");
                    using var connection = new SqlConnection(masterConnectionString);
                    await connection.OpenAsync();
                    logger.LogInformation("Connected to master database.");

                    using var command = connection.CreateCommand();
                    command.CommandText = $@"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
                        BEGIN
                            CREATE DATABASE [{databaseName}]
                        END
                        ELSE
                        BEGIN
                            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            ALTER DATABASE [{databaseName}] SET MULTI_USER;
                        END";
                    await command.ExecuteNonQueryAsync();

                    logger.LogInformation($"Ensured database {databaseName} exists and is accessible.");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Database connection attempt {i + 1} failed. Error: {ex.Message}");
                    if (i < maxRetryCount - 1)
                    {
                        logger.LogInformation($"Waiting {retryInterval.TotalSeconds} seconds before next attempt...");
                        await Task.Delay(retryInterval);
                    }
                    else
                    {
                        logger.LogError("All database connection attempts failed.");
                        throw;
                    }
                }
            }
        }
    }
}