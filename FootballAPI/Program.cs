using FootballAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using FootballAPI.Core.Interfaces;
using FootballAPI.Infraestructure.Repositories;
using FootballAPI.Services;
using FootballAPI;
using FootballAPI.Infraestructure.Data.MongoDB;
using FootballAPI.Infraestructure.Repositories.MongoDB;
using FootballAPI.Infrastructure.Repositories.MongoDB;
using FootballAPI.Infraestructure.Data.SQLServer;
using FootballAPI.Infraestructure.Repositories.SqlServer;
using FootballAPI.Infraestructure.Repositories.Factories;
using FootballAPI.Core.Interfaces.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Determine which database to use
string databaseType = builder.Configuration.GetValue<string>("DatabaseType") ?? "SqlServer";
bool useMongoDB = databaseType.Equals("MongoDB", StringComparison.OrdinalIgnoreCase);

// 1. Register contexts first
if (useMongoDB)
{
    builder.Services.AddSingleton<MongoDbContext>(sp => 
        new MongoDbContext(builder.Configuration));
}
else
{
    builder.Services.AddSingleton<SqlServerDbContext>(sp =>
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        
        var connBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            TrustServerCertificate = true,
            MultipleActiveResultSets = true,
            ConnectTimeout = 30
        };

        optionsBuilder.UseSqlServer(connBuilder.ConnectionString, options => 
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            options.CommandTimeout(60);
        });

        return new SqlServerDbContext(optionsBuilder.Options);
    });
}

// 2. Register repositories and their interfaces
if (useMongoDB)
{
    // Base repositories
    builder.Services.AddSingleton(typeof(MongoRepository<>));
    builder.Services.AddSingleton(typeof(IRepository<>), typeof(MongoRepository<>));
    
    // Specialized repositories
    builder.Services.AddSingleton<MongoPlayerRepository>();
    builder.Services.AddSingleton<MongoManagerRepository>();
    
    // Register interfaces
    builder.Services.AddSingleton<IPlayerRepository>(sp => 
        sp.GetRequiredService<MongoPlayerRepository>());
    builder.Services.AddSingleton<IManagerRepository>(sp => 
        sp.GetRequiredService<MongoManagerRepository>());
}
else
{
    // Base repositories
    builder.Services.AddSingleton(typeof(SqlServerRepository<>));
    builder.Services.AddSingleton(typeof(IRepository<>), typeof(SqlServerRepository<>));
    
    // Specialized repositories
    builder.Services.AddSingleton<SqlServerPlayerRepository>();
    builder.Services.AddSingleton<SqlServerManagerRepository>();
    
    // Register interfaces
    builder.Services.AddSingleton<IPlayerRepository>(sp => 
        sp.GetRequiredService<SqlServerPlayerRepository>());
    builder.Services.AddSingleton<IManagerRepository>(sp => 
        sp.GetRequiredService<SqlServerManagerRepository>());
}

// 3. Register database factories
builder.Services.AddSingleton<SqlServerDatabaseFactory>();
builder.Services.AddSingleton<MongoDbDatabaseFactory>();

// 4. Register the repository factory
builder.Services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

// 5. Register Unit of Work
if (useMongoDB)
{
    builder.Services.AddSingleton<MongoUnitOfWork>();
    builder.Services.AddSingleton<IUnitOfWork>(sp => 
        sp.GetRequiredService<MongoUnitOfWork>());
}
else
{
    builder.Services.AddSingleton<SqlServerUnitOfWork>();
    builder.Services.AddSingleton<IUnitOfWork>(sp => 
        sp.GetRequiredService<SqlServerUnitOfWork>());
}

// 6. Register other services
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddHttpClient("AlignmentAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AlignmentAPI:BaseUrl"]);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// 7. Register application services
builder.Services.AddSingleton<IMatchAlignmentService, MatchAlignmentService>();
builder.Services.AddHostedService<BackgroundAlignmentService>();

// 8. Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        app.Urls.Clear();
        app.Urls.Add("http://+:80");
    }
}

app.UseAuthorization();
app.MapControllers();

// Initialize the database based on type
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        
        if (useMongoDB)
        {
            logger.LogInformation("Initializing MongoDB database...");
            var mongoContext = services.GetRequiredService<MongoDbContext>();
            await MongoDbInitializer.Initialize(mongoContext, configuration, logger);
            logger.LogInformation("MongoDB initialization completed successfully.");
        }
        else
        {
            logger.LogInformation("Initializing SQL Server database...");
            var sqlContext = services.GetRequiredService<SqlServerDbContext>();

            // Initialize database with retry
            var retryStrategy = sqlContext.Database.CreateExecutionStrategy();
            await retryStrategy.ExecuteAsync(async () =>
            {
                await SqlServerDbInitializer.Initialize(sqlContext, configuration, logger);
                logger.LogInformation("SQL Server initialization completed successfully.");
            });
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

// Log application startup
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Application started successfully using {DatabaseType} database.", 
        useMongoDB ? "MongoDB" : "SQL Server");
    logger.LogInformation("Match alignment service will check for upcoming matches every 5 minutes.");
});

await app.RunAsync();