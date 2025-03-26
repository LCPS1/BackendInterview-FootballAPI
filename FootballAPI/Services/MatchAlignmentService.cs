using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using FootballAPI.Core.Interfaces;
using FootballAPI.Models;
using FootballAPI.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FootballAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using FootballAPI.Infraestructure.Data.SQLServer;
using System.Text.Json;
using FootballAPI.Core.Interfaces.Factories;

namespace FootballAPI.Services
{
    // TODO: Enhance this service with a robust queue system for managing notifications
    // This would include:
    // - A ConcurrentQueue<int> for storing match IDs
    // - Parallel processing with controlled concurrency
    // - Retry mechanism for failed API calls
    // - Proper error handling and recovery
    /// <summary>
    /// Service for checking and notifying about match alignments
    /// </summary>
    /// <remarks>
    /// Uses repository factory pattern to support multiple database types
    /// </remarks>
    public class MatchAlignmentService : IMatchAlignmentService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<MatchAlignmentService> _logger;
        private readonly HttpClient _httpClient;

        public MatchAlignmentService(
            IRepositoryFactory repositoryFactory,
            ILogger<MatchAlignmentService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("AlignmentAPI");
        }

        /// <summary>
        /// Checks if a specific match has correct alignment
        /// </summary>
        /// <param name="matchId">The ID of the match to check</param>
        /// <returns>True if alignment is correct, false otherwise</returns>
        public async Task<bool> CheckMatchAlignmentAsync(int matchId)
        {
            try
            {
                _logger.LogInformation($"Checking alignment for match {matchId}");
                
                var matchRepository = _repositoryFactory.CreateRepository<Match>();
                
                var matches = await matchRepository.FindWithIncludeAsync(
                    m => m.Id == matchId,
                    "HouseManager",
                    "AwayManager",
                    "Referee",
                    "HousePlayers",
                    "AwayPlayers"
                );
                
                var match = matches.FirstOrDefault();
                
                if (match == null)
                {
                    _logger.LogWarning($"Match {matchId} not found");
                    return false;
                }
                
                bool hasCorrectAlignment = !HasIncorrectAlignment(match);
                
                if (!hasCorrectAlignment)
                {
                    await NotifyIncorrectAlignmentAsync(matchId);
                }
                
                return hasCorrectAlignment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking alignment for match {matchId}");
                return false;
            }
        }

        /// <summary>
        /// Checks upcoming matches and notifies about incorrect alignments
        /// </summary>
        /// <returns>Number of notifications sent</returns>
        public async Task<int> NotifyIncorrectAlignmentsAsync()
        {
            int notificationsSent = 0;
            
            try
            {
                var now = DateTime.UtcNow;
                var fiveMinutesLater = now.AddMinutes(5);
                
                _logger.LogInformation($"Looking for matches scheduled between {now} and {fiveMinutesLater}");
                
                var matchRepository = _repositoryFactory.CreateRepository<Match>();
                
                var upcomingMatches = await matchRepository.FindWithIncludeAsync(
                    m => m.ScheduledStart >= now && 
                         m.ScheduledStart <= fiveMinutesLater &&
                         m.Status == MatchStatus.Scheduled,
                    "HouseManager",
                    "AwayManager",
                    "Referee",
                    "HousePlayers",
                    "AwayPlayers"
                );
                
                var matchList = upcomingMatches.ToList();
                _logger.LogInformation($"Found {matchList.Count} upcoming matches to check");
                
                foreach (var match in matchList)
                {
                    _logger.LogInformation($"Checking alignment for match {match.Id} scheduled at {match.ScheduledStart}");
                    
                    if (HasIncorrectAlignment(match))
                    {
                        await NotifyIncorrectAlignmentAsync(match.Id);
                        _logger.LogWarning($"Notification sent: Match {match.Id} has incorrect alignment");
                        notificationsSent++;
                    }
                    else
                    {
                        _logger.LogInformation($"Match {match.Id} has correct alignment");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NotifyIncorrectAlignmentsAsync");
            }
            
            return notificationsSent;
        }

        /// <summary>
        /// Checks if a match has incorrect alignment based on defined rules
        /// </summary>
        /// <param name="match">The match to check</param>
        /// <returns>True if alignment is incorrect, false otherwise</returns>
        private bool HasIncorrectAlignment(Match match)
        {
            var issues = new List<string>();
            
            // Rule 1: Each team must have exactly 11 players
            if (match.HousePlayers?.Count != 11)
            {
                issues.Add($"Home team has {match.HousePlayers?.Count ?? 0} players instead of 11");
            }
            
            if (match.AwayPlayers?.Count != 11)
            {
                issues.Add($"Away team has {match.AwayPlayers?.Count ?? 0} players instead of 11");
            }
            
            // Rule 2: Match must have managers assigned
            if (match.HouseManager == null)
            {
                issues.Add("Home team has no manager assigned");
            }
            
            if (match.AwayManager == null)
            {
                issues.Add("Away team has no manager assigned");
            }
            
            // Rule 3: Match must have a referee
            if (match.Referee == null)
            {
                issues.Add("Match has no referee assigned");
            }
            
            // Log all issues found
            if (issues.Any())
            {
                _logger.LogWarning($"Match {match.Id} has incorrect alignment:");
                foreach (var issue in issues)
                {
                    _logger.LogWarning($"- {issue}");
                }
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Notifies external API about incorrect alignment
        /// </summary>
        /// <param name="matchId">The ID of the match with incorrect alignment</param>
        private async Task NotifyIncorrectAlignmentAsync(int matchId)
        {
            try
            {
                var requestData = new[] { matchId };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    System.Text.Encoding.UTF8,
                    "application/json-patch+json");
                
                var response = await _httpClient.PostAsync("api/IncorrectAlignment", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully notified API about incorrect alignment for match {matchId}");
                }
                else
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to notify API about incorrect alignment for match {matchId}. Status: {response.StatusCode}, Response: {responseText}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying incorrect alignment for match {matchId}");
                throw; // Re-throw to allow caller to handle the error
            }
        }
    }
}
