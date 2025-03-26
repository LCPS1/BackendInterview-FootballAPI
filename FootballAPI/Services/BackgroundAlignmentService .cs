using System;
using System.Threading;
using System.Threading.Tasks;
using FootballAPI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FootballAPI.Services
{ 
    public class BackgroundAlignmentService : BackgroundService
    {
        private readonly ILogger<BackgroundAlignmentService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BackgroundAlignmentService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundAlignmentService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background alignment service starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var alignmentService = scope.ServiceProvider.GetRequiredService<IMatchAlignmentService>();
                        var notificationCount = await alignmentService.NotifyIncorrectAlignmentsAsync();
                        _logger.LogInformation("Found {Count} matches with incorrect alignments", notificationCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing background alignment check");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}