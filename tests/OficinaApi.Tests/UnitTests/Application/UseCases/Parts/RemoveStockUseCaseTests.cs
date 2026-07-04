using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.UseCases.Parts;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Application.UseCases.Parts
{
    public class RemoveStockUseCaseTests
    {
        private readonly Mock<IPartRepository> _partRepoMock;
        private readonly RemoveStockUseCase _sut;

        public RemoveStockUseCaseTests()
        {
            _partRepoMock = new Mock<IPartRepository>();
            _sut = new RemoveStockUseCase(_partRepoMock.Object, Mock.Of<ILogger<RemoveStockUseCase>>());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldDecreaseQuantity_WhenEnoughStock()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var part = new Part("Brake pad", "BP-1", 10, 45.0m);
            _partRepoMock.Setup(x => x.GetByIdAsync(partId)).ReturnsAsync(part);

            // Act
            var result = await _sut.ExecuteAsync(new RemoveStockRequest(partId, 3));

            // Assert
            result.IsSuccess.Should().BeTrue();
            part.QuantityInStock.Should().Be(7);
            _partRepoMock.Verify(x => x.UpdateAsync(part), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnFailure_WhenNotEnoughStock()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var part = new Part("Tire", "TR-1", 2, 200.0m);
            _partRepoMock.Setup(x => x.GetByIdAsync(partId)).ReturnsAsync(part);

            // Act
            var result = await _sut.ExecuteAsync(new RemoveStockRequest(partId, 3));

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Messages.Should().Contain("Estoque insuficiente para remover essa quantidade.");
        }
    }
}
