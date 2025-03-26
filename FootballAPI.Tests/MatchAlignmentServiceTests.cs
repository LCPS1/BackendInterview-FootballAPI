using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;
using FootballAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace FootballAPI.Tests
{
    public class MatchAlignmentServiceTests
    {
        private readonly Mock<IRepositoryFactory> _repositoryFactoryMock;
        private readonly Mock<IRepository<Core.Entities.Match>> _matchRepositoryMock;
        private readonly Mock<ILogger<MatchAlignmentService>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public MatchAlignmentServiceTests()
        {
            // Setup repository mocks
            _repositoryFactoryMock = new Mock<IRepositoryFactory>();
            _matchRepositoryMock = new Mock<IRepository<Core.Entities.Match>>();
            _repositoryFactoryMock
                .Setup(f => f.CreateRepository<Core.Entities.Match>())
                .Returns(_matchRepositoryMock.Object);

            // Setup logger mock
            _loggerMock = new Mock<ILogger<MatchAlignmentService>>();

            // Setup HTTP mocks
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/")
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(factory => factory.CreateClient("AlignmentAPI"))
                .Returns(httpClient);
        }

        [Fact]
        public async Task CheckMatchAlignment_CorrectAlignment_ReturnsTrue()
        {
            // Arrange
            var match = CreateValidMatch();
            SetupMatchRepository(match);

            var service = new MatchAlignmentService(
                _repositoryFactoryMock.Object,
                _loggerMock.Object,
                _httpClientFactoryMock.Object);

            // Act
            var result = await service.CheckMatchAlignmentAsync(1);

            // Assert
            Assert.True(result);
            VerifyNoHttpCall();
        }

        [Fact]
        public async Task CheckMatchAlignment_MissingPlayers_ReturnsFalse()
        {
            // Arrange
            var match = CreateInvalidMatch_MissingPlayers();
            SetupMatchRepository(match);
            SetupHttpSuccess();

            var service = new MatchAlignmentService(
                _repositoryFactoryMock.Object,
                _loggerMock.Object,
                _httpClientFactoryMock.Object);

            // Act
            var result = await service.CheckMatchAlignmentAsync(1);

            // Assert
            Assert.False(result);
            VerifyHttpCall();
        }

        private void SetupMatchRepository(Core.Entities.Match match)
        {
            _matchRepositoryMock
                .Setup(r => r.FindWithIncludeAsync(
                    It.IsAny<Expression<Func<Core.Entities.Match, bool>>>(),
                    It.Is<string[]>(includes =>
                        includes.Contains("HouseManager") &&
                        includes.Contains("AwayManager") &&
                        includes.Contains("Referee") &&
                        includes.Contains("HousePlayers") &&
                        includes.Contains("AwayPlayers"))))
                .ReturnsAsync(new List<Core.Entities.Match> { match });
        }

        private Core.Entities.Match CreateValidMatch()
        {
            return new Core.Entities.Match
            {
                Id = 1,
                ScheduledStart = DateTime.UtcNow.AddMinutes(3),
                Status = MatchStatus.Scheduled,
                Referee = new Referee { Id = 1, Name = "Test Referee" },
                HouseManager = new Manager { Id = 1, Name = "Home Manager" },
                AwayManager = new Manager { Id = 2, Name = "Away Manager" },
                HousePlayers = Enumerable.Range(1, 11)
                    .Select(i => new Player { Id = i, Name = $"Home Player {i}" })
                    .ToList(),
                AwayPlayers = Enumerable.Range(12, 11)
                    .Select(i => new Player { Id = i, Name = $"Away Player {i}" })
                    .ToList()
            };
        }

        private Core.Entities.Match CreateInvalidMatch_MissingPlayers()
        {
            var match = CreateValidMatch();
            match.HousePlayers = match.HousePlayers.Take(10).ToList();
            return match;
        }

        private void SetupHttpSuccess()
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
        }

        private void VerifyHttpCall()
        {
            _httpMessageHandlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("api/IncorrectAlignment")),
                    ItExpr.IsAny<CancellationToken>());
        }

        private void VerifyNoHttpCall()
        {
            _httpMessageHandlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}