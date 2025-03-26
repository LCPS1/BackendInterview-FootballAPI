using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballAPI.Core.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FootballAPI.Infraestructure.Data.MongoDB
{
 public static class MongoDbInitializer
{
    public static async Task Initialize(MongoDbContext context, IConfiguration configuration, ILogger logger)
    {
        try
        {
            // Ensure database exists and collections are created
            await EnsureDatabaseExists(context, configuration, logger);

            // Check if we need to seed
            var playerCollection = context.GetCollection<Player>();
            if (await playerCollection.CountDocumentsAsync(FilterDefinition<Player>.Empty) > 0)
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
                // ... same players as SQL version ...
            };
            await context.GetCollection<Player>().InsertManyAsync(players);

            var managers = new List<Manager>
            {
                new() { Name = "Alex" },
                new() { Name = "Zidane" },
                new() { Name = "Guardiola" }
            };
            await context.GetCollection<Manager>().InsertManyAsync(managers);

            var referees = new List<Referee>
            {
                new() { Name = "Pierluigi" },
                new() { Name = "Howard" }
            };
            await context.GetCollection<Referee>().InsertManyAsync(referees);

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
                    HousePlayers = players.Take(5).ToList(),
                    AwayPlayers = players.Skip(11).Take(7).ToList(),
                    ScheduledStart = DateTime.UtcNow.AddMinutes(3),
                    Status = MatchStatus.Scheduled
                }
            };
            await context.GetCollection<Match>().InsertManyAsync(matches);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization.");
            throw;
        }
    }

    private static async Task EnsureMatchesHaveScheduledStart(MongoDbContext context, ILogger logger)
    {
        try
        {
            var matchCollection = context.GetCollection<Match>();
            var filter = Builders<Match>.Filter.Eq(m => m.ScheduledStart, default(DateTime));
            var update = Builders<Match>.Update
                .Set(m => m.ScheduledStart, DateTime.UtcNow.AddMinutes(new Random().Next(2, 10)));

            var result = await matchCollection.UpdateManyAsync(filter, update);
            
            if (result.ModifiedCount > 0)
            {
                logger.LogInformation("Updated {Count} matches with ScheduledStart times", result.ModifiedCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating match scheduled times");
            throw;
        }
    }

    private static async Task EnsureDatabaseExists(MongoDbContext context, IConfiguration configuration, ILogger logger)
    {
        try
        {
            var maxRetryCount = 10;
            var retryInterval = TimeSpan.FromSeconds(5);

            // Add initial delay to allow MongoDB to fully start
            await Task.Delay(TimeSpan.FromSeconds(15));

            for (int i = 0; i < maxRetryCount; i++)
            {
                try
                {
                    logger.LogInformation($"Attempting connection to MongoDB (attempt {i + 1} of {maxRetryCount})...");
                    
                    // Test connection
                    await context.GetDatabase().RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                    
                    // Create collections if they don't exist
                    var db = context.GetDatabase();
                    var collections = (await db.ListCollectionNamesAsync()).ToList();

                    if (!collections.Contains("Players"))
                        await db.CreateCollectionAsync("Players");
                    if (!collections.Contains("Managers"))
                        await db.CreateCollectionAsync("Managers");
                    if (!collections.Contains("Referees"))
                        await db.CreateCollectionAsync("Referees");
                    if (!collections.Contains("Matches"))
                        await db.CreateCollectionAsync("Matches");

                    // Create indexes
                    await CreateIndexes(context, logger);

                    logger.LogInformation("MongoDB database and collections ensured.");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Database connection attempt {i + 1} failed.");
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring database exists");
            throw;
        }
    }

    private static async Task CreateIndexes(MongoDbContext context, ILogger logger)
    {
        try
        {
            var playerCollection = context.GetCollection<Player>();
            var managerCollection = context.GetCollection<Manager>();
            var refereeCollection = context.GetCollection<Referee>();
            var matchCollection = context.GetCollection<Match>();

            await playerCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Player>(Builders<Player>.IndexKeys.Ascending(p => p.Name)));
            
            await matchCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<Match>(Builders<Match>.IndexKeys.Ascending(m => m.ScheduledStart)));

            logger.LogInformation("Created MongoDB indexes");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating indexes");
            // Continue execution - indexes are for performance, not functionality
        }
    }
}
}