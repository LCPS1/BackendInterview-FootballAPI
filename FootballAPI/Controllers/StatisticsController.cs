using FootballAPI.Core.Entities;
using Microsoft.AspNetCore.Identity;
using FootballAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Mvc;
using FootballAPI.DTOs;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;

namespace FootballAPI.Controllers
{
   [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            IRepositoryFactory repositoryFactory,
            ILogger<StatisticsController> logger)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets yellow card statistics for players and managers
        /// </summary>
        [HttpGet("yellowcards")]
        public async Task<IActionResult> GetYellowCards([FromQuery] int limit = 10)
        {
            try
            {
                // Get specialized repositories from factory
                var playerRepository = _repositoryFactory.CreatePlayerRepository();
                var managerRepository = _repositoryFactory.CreateManagerRepository();

                // Get players with yellow cards
                var players = await playerRepository.GetTopCardHoldersAsync(yellowCard: true, count: limit);
                
                var playerStats = players.Select(p => new CardStatisticsDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CardCount = p.YellowCard,
                    Type = "Player"
                });

                // Get managers with yellow cards
                var managers = await managerRepository.GetTopYellowCardHoldersAsync(limit);
                
                var managerStats = managers.Select(m => new CardStatisticsDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    CardCount = m.YellowCard,
                    Type = "Manager"
                });

                // Combine player and manager stats and take top results
                var combinedStats = playerStats.Concat(managerStats)
                    .OrderByDescending(s => s.CardCount)
                    .Take(limit);

                return Ok(combinedStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving yellow card statistics");
                return StatusCode(500, "An error occurred");
            }
        }

        /// <summary>
        /// Gets red card statistics for players and managers
        /// </summary>
        [HttpGet("redcards")]
        public async Task<IActionResult> GetRedCards([FromQuery] int limit = 10)
        {
            try
            {
                // Get specialized repositories from factory
                var playerRepository = _repositoryFactory.CreatePlayerRepository();
                var managerRepository = _repositoryFactory.CreateManagerRepository();

                // Get players with red cards
                var players = await playerRepository.GetTopCardHoldersAsync(yellowCard: false, count: limit);
                
                var playerStats = players.Select(p => new CardStatisticsDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CardCount = p.RedCard,
                    Type = "Player"
                });

                // Get managers with red cards
                var managers = await managerRepository.GetTopRedCardHoldersAsync(limit);
                
                var managerStats = managers.Select(m => new CardStatisticsDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    CardCount = m.RedCard,
                    Type = "Manager"
                });

                // Combine player and manager stats and take top results
                var combinedStats = playerStats.Concat(managerStats)
                    .OrderByDescending(s => s.CardCount)
                    .Take(limit);

                return Ok(combinedStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving red card statistics");
                return StatusCode(500, "An error occurred");
            }
        }

        /// <summary>
        /// Gets minutes played statistics for players
        /// </summary>
        [HttpGet("minutesplayed")]
        public async Task<IActionResult> GetMinutesPlayed([FromQuery] int limit = 10)
        {
            try
            {
                // Get specialized player repository from factory
                var playerRepository = _repositoryFactory.CreatePlayerRepository();

                // Get players ordered by minutes played
                var players = await playerRepository.GetTopByMinutesPlayedAsync(count: limit);
                
                // Map to statistics DTOs
                var playerStats = players.Select(p => new MinutesPlayedStatisticsDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    MinutesPlayed = p.MinutesPlayed
                });
                
                return Ok(playerStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving minutes played statistics");
                return StatusCode(500, "An error occurred");
            }
        }
    }
}



