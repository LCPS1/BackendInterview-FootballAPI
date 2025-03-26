using FootballAPI.Core.Interfaces;
using FootballAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Tests
{
    public class BackgroundAlignmentServiceTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMatchAlignmentService> _alignmentServiceMock;
        private readonly Mock<ILogger<BackgroundAlignmentService>> _loggerMock;

        public BackgroundAlignmentServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _alignmentServiceMock = new Mock<IMatchAlignmentService>();
            _loggerMock = new Mock<ILogger<BackgroundAlignmentService>>();

            // Setup scope factory
            _serviceScopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(_scopeMock.Object);

            // Setup service provider to return scope factory
            _serviceProviderMock
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_serviceScopeFactoryMock.Object);

            // Setup scope
            _scopeMock
                .Setup(x => x.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            // Setup service provider to return alignment service
            _serviceProviderMock
                .Setup(x => x.GetService(typeof(IMatchAlignmentService)))
                .Returns(_alignmentServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_CallsAlignmentService_AndLogsResults()
        {
            // Arrange
            _alignmentServiceMock
                .Setup(x => x.NotifyIncorrectAlignmentsAsync())
                .ReturnsAsync(2); // Simulate 2 incorrect alignments found

            var service = new BackgroundAlignmentService(
                _serviceProviderMock.Object,
                _loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await service.StartAsync(cancellationTokenSource.Token);

            // Wait for first execution
            await Task.Delay(100);

            // Cancel the operation
            cancellationTokenSource.Cancel();

            // Wait for service to stop
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _alignmentServiceMock.Verify(
                x => x.NotifyIncorrectAlignmentsAsync(),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Found 2 matches")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_HandlesAndLogsErrors()
        {
            // Arrange
            var expectedException = new Exception("Test exception");
            _alignmentServiceMock
                .Setup(x => x.NotifyIncorrectAlignmentsAsync())
                .ThrowsAsync(expectedException);

            var service = new BackgroundAlignmentService(
                _serviceProviderMock.Object,
                _loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await service.StartAsync(cancellationTokenSource.Token);

            // Wait for first execution
            await Task.Delay(100);

            // Cancel the operation
            cancellationTokenSource.Cancel();

            // Wait for service to stop
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error performing background")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task StopAsync_CancelsExecution()
        {
            // Arrange
            var service = new BackgroundAlignmentService(
                _serviceProviderMock.Object,
                _loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await service.StartAsync(cancellationTokenSource.Token);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Background alignment service starting")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_RespectsDelayInterval()
        {
            // Arrange
            var executionCount = 0;
            _alignmentServiceMock
                .Setup(x => x.NotifyIncorrectAlignmentsAsync())
                .Callback(() => executionCount++)
                .ReturnsAsync(0);

            var service = new BackgroundAlignmentService(
                _serviceProviderMock.Object,
                _loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await service.StartAsync(cancellationTokenSource.Token);

            // Wait for slightly more than one interval
            await Task.Delay(TimeSpan.FromMilliseconds(1100));

            cancellationTokenSource.Cancel();
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            Assert.True(executionCount >= 1, "Service should execute at least once");
            Assert.True(executionCount <= 2, "Service should not execute too many times within the interval");
        }

        [Fact]
        public async Task ExecuteAsync_DisposesScope()
        {
            // Arrange
            var service = new BackgroundAlignmentService(
                _serviceProviderMock.Object,
                _loggerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await service.StartAsync(cancellationTokenSource.Token);

            // Wait for first execution
            await Task.Delay(100);

            cancellationTokenSource.Cancel();
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _scopeMock.Verify(x => x.Dispose(), Times.AtLeastOnce);
        }
    }
}