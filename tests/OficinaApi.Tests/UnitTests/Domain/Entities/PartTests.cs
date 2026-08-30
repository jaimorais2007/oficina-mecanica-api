using FluentAssertions;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Events;
using OficinaApi.Domain.Exceptions;
using Xunit;

namespace Unit.Tests;

public class PartTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetAllProperties()
    {
        // Arrange
        var name = "Filtro de Óleo";
        var code = "FO-001";
        var initialQuantity = 10;
        var price = 29.90m;
        var before = DateTime.UtcNow;

        // Act
        var part = new Part(name, code, initialQuantity, price);

        // Assert
        part.Id.Should().NotBeEmpty();
        part.Name.Should().Be(name);
        part.Code.Should().Be(code);
        part.QuantityInStock.Should().Be(initialQuantity);
        part.Price.Should().Be(price);
        part.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeEmptyServiceOrdersParts()
    {
        // Arrange & Act
        var part = new Part("Filtro", "F-001", 5, 10m);

        // Assert
        part.ServiceOrdersParts.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldHaveNoDomainEvents()
    {
        // Arrange & Act
        var part = new Part("Filtro", "F-001", 5, 10m);

        // Assert
        part.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddStock_WithPositiveQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        part.AddStock(5);

        // Assert
        part.QuantityInStock.Should().Be(15);
    }

    [Fact]
    public void AddStock_WithPositiveQuantity_ShouldRaisePartStockAddedEvent()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        part.AddStock(5);

        // Assert
        part.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PartStockAddedEvent>()
            .Which.PartId.Should().Be(part.Id);
    }

    [Fact]
    public void AddStock_WithZeroQuantity_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        var act = () => part.AddStock(0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void AddStock_WithNegativeQuantity_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        var act = () => part.AddStock(-3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void RemoveStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        part.RemoveStock(4);

        // Assert
        part.QuantityInStock.Should().Be(6);
    }

    [Fact]
    public void RemoveStock_WithExactStockQuantity_ShouldZeroOutStock()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 5, 29.90m);

        // Act
        part.RemoveStock(5);

        // Assert
        part.QuantityInStock.Should().Be(0);
    }

    [Fact]
    public void RemoveStock_WithZeroQuantity_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        var act = () => part.RemoveStock(0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void RemoveStock_WithNegativeQuantity_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 10, 29.90m);

        // Act
        var act = () => part.RemoveStock(-2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void RemoveStock_WhenQuantityExceedsStock_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro de Óleo", "FO-001", 3, 29.90m);

        // Act
        var act = () => part.RemoveStock(10);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public void UpdateDetails_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var part = new Part("Nome Antigo", "COD-OLD", 5, 10m);

        // Act
        part.UpdateDetails("Nome Novo", "COD-NEW", 49.99m);

        // Assert
        part.Name.Should().Be("Nome Novo");
        part.Code.Should().Be("COD-NEW");
        part.Price.Should().Be(49.99m);
    }

    [Fact]
    public void UpdateDetails_WithZeroPrice_ShouldUpdateSuccessfully()
    {
        // Arrange
        var part = new Part("Filtro", "F-001", 5, 10m);

        // Act
        part.UpdateDetails("Filtro", "F-001", 0m);

        // Assert
        part.Price.Should().Be(0m);
    }

    [Fact]
    public void UpdateDetails_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro", "F-001", 5, 10m);

        // Act
        var act = () => part.UpdateDetails(null!, "F-001", 10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Nome invalido*");
    }

    [Fact]
    public void UpdateDetails_WithWhitespaceName_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro", "F-001", 5, 10m);

        // Act
        var act = () => part.UpdateDetails("   ", "F-001", 10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Nome invalido*");
    }

    [Fact]
    public void UpdateDetails_WithNegativePrice_ShouldThrowDomainException()
    {
        // Arrange
        var part = new Part("Filtro", "F-001", 5, 10m);

        // Act
        var act = () => part.UpdateDetails("Filtro", "F-001", -1m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*negativo*");
    }
}
