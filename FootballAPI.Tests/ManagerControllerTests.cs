using FootballAPI.Controllers;
using FootballAPI.Core.Entities;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;
using FootballAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Tests
{
    public class ManagerControllerTests
    {
        private readonly Mock<IRepositoryFactory> _mockFactory;
        private readonly Mock<IRepository<Manager>> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<ManagerController>> _mockLogger;
        private readonly ManagerController _controller;

        public ManagerControllerTests()
        {
            _mockFactory = new Mock<IRepositoryFactory>();
            _mockRepository = new Mock<IRepository<Manager>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ManagerController>>();

            // Setup factory to return our mocked repository
            _mockFactory.Setup(f => f.CreateRepository<Manager>())
                .Returns(_mockRepository.Object);
            _mockFactory.Setup(f => f.CreateUnitOfWork())
                .Returns(_mockUnitOfWork.Object);

            _controller = new ManagerController(_mockFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public void Get_ReturnsOkResult_WithListOfManagers()
        {
            // Arrange
            var managers = new List<Manager>
            {
                new Manager { Id = 1, Name = "Test Manager 1" },
                new Manager { Id = 2, Name = "Test Manager 2" }
            };

            _mockRepository.Setup(r => r.GetAll())
                .Returns(managers.AsQueryable());

            // Act
            var result = _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedManagers = Assert.IsAssignableFrom<IEnumerable<Manager>>(okResult.Value);
            Assert.Equal(2, returnedManagers.Count());
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenManagerExists()
        {
            // Arrange
            var manager = new Manager { Id = 1, Name = "Test Manager" };
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(manager);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedManager = Assert.IsType<Manager>(okResult.Value);
            Assert.Equal(manager.Id, returnedManager.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenManagerDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Manager)null);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Post_ReturnsCreatedAtRoute_WhenManagerIsValid()
        {
            // Arrange
            var manager = new Manager { Name = "New Manager" };
            var createdManager = new Manager { Id = 1, Name = "New Manager" };

            _mockRepository.Setup(r => r.AddAsync(manager))
                .ReturnsAsync(createdManager);

            // Act
            var result = await _controller.Post(manager);

            // Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetManagerById", createdAtRouteResult.RouteName);
            Assert.Equal(1, createdAtRouteResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var manager = new Manager { Id = 1, Name = "Updated Manager" };
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(manager);

            // Act
            var result = await _controller.Update(1, manager);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepository.Verify(r => r.UpdateAsync(manager), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleteIsSuccessful()
        {
            // Arrange
            var manager = new Manager { Id = 1, Name = "Manager to Delete" };
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(manager);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenManagerDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Manager)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenManagerIsNull()
        {
            // Act
            var result = await _controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var manager = new Manager { Id = 2, Name = "Test Manager" };

            // Act
            var result = await _controller.Update(1, manager);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}