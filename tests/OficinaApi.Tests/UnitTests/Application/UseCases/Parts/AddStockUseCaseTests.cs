using System;
using System.Collections.Generic;
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
    public class AddStockUseCaseTests
    {
        private readonly Mock<IPartRepository> _partRepoMock;
        private readonly AddStockUseCase _sut;

        public AddStockUseCaseTests()
        {
            _partRepoMock = new Mock<IPartRepository>();
            _sut = new AddStockUseCase(_partRepoMock.Object, Mock.Of<ILogger<AddStockUseCase>>());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldIncreaseQuantity()
        {
            // Arrange
            var partId = Guid.NewGuid();
            var part = new Part("Oil filter", "OF-1", 10, 25.0m);
            _partRepoMock.Setup(x => x.GetByIdAsync(partId)).ReturnsAsync(part);

            // Act
            var result = await _sut.ExecuteAsync(new AddStockRequest(partId, 5));

            // Assert
            result.IsSuccess.Should().BeTrue();
            part.QuantityInStock.Should().Be(15);
            _partRepoMock.Verify(x => x.UpdateAsync(part), Times.Once);
        }
    }
}
