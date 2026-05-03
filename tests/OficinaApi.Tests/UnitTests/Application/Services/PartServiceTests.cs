using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using Xunit;

namespace OficinaApi.Tests;

public class PartServiceTests
{
    private readonly Mock<IPartRepository> _partRepoMock;
    private readonly PartService _sut;

    public PartServiceTests()
    {
        _partRepoMock = new Mock<IPartRepository>();
        _sut = new PartService(_partRepoMock.Object);
    }

    [Fact]
    public async Task AddStockAsync_ShouldIncreaseQuantity()
    {
        // Arrange
        var partId = Guid.NewGuid();
        var part = new Part("Oil filter", "OF-1", 10, 25.0m);
        _partRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(part);

        // Act
        await _sut.AddStockAsync(partId, 5);

        // Assert
        part.QuantityInStock.Should().Be(15);
        _partRepoMock.Verify(x => x.UpdateAsync(part), Times.Once);
    }

    [Fact]
    public async Task RemoveStockAsync_ShouldDecreaseQuantity_WhenEnoughStock()
    {
        // Arrange
        var partId = Guid.NewGuid();
        var part = new Part("Brake pad", "BP-1", 10, 45.0m);
        _partRepoMock.Setup(x => x.GetByIdAsync(partId)).ReturnsAsync(part);

        // Act
        await _sut.RemoveStockAsync(partId, 3);

        // Assert
        part.QuantityInStock.Should().Be(7);
        _partRepoMock.Verify(x => x.UpdateAsync(part), Times.Once);
    }

    [Fact]
    public async Task RemoveStockAsync_ShouldThrowException_WhenNotEnoughStock()
    {
        // Arrange
        var partId = Guid.NewGuid();
        var part = new Part("Tire", "TR-1", 2, 200.0m);
        _partRepoMock.Setup(x => x.GetByIdAsync(partId)).ReturnsAsync(part);

        // Act
        Func<Task> act = async () => await _sut.RemoveStockAsync(partId, 3);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Estoque insuficiente para remover essa quantidade.");
    }
}
