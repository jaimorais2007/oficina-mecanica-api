using Xunit;
using FluentAssertions;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.ValueObjects;

namespace Unit.Tests;

public class PlateTests
{
    [Fact]
    public void Constructor_WhenOldFormatPlateIsValid_ShouldCreatePlate()
    {
        // Arrange
        var validPlate = "ABC1234";

        // Act
        var plate = new Plate(validPlate);

        // Assert
        Assert.Equal("ABC1234", plate.Value);
    }

    [Fact]
    public void Constructor_WhenOldFormatPlateIsLowercase_ShouldStoreAsUppercase()
    {
        // Arrange
        var lowercasePlate = "abc1234";

        // Act
        var plate = new Plate(lowercasePlate);

        // Assert
        Assert.Equal("ABC1234", plate.Value);
    }

    [Fact]
    public void Constructor_WhenMercosulFormatPlateIsValid_ShouldCreatePlate()
    {
        // Arrange
        var mercosulPlate = "ABC1D23";

        // Act
        var plate = new Plate(mercosulPlate);

        // Assert
        Assert.Equal("ABC1D23", plate.Value);
    }

    [Fact]
    public void Constructor_WhenMercosulFormatPlateIsLowercase_ShouldStoreAsUppercase()
    {
        // Arrange
        var lowercaseMercosul = "abc1d23";

        // Act
        var plate = new Plate(lowercaseMercosul);

        // Assert
        Assert.Equal("ABC1D23", plate.Value);
    }

    [Fact]
    public void Constructor_WhenPlateIsNull_ShouldThrowDomainException()
    {
        // Arrange
        string? nullPlate = null;

        // Act
        var act = () => new Plate(nullPlate!);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Constructor_WhenPlateIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var emptyPlate = string.Empty;

        // Act
        var act = () => new Plate(emptyPlate);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Constructor_WhenPlateIsWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        var whitespacePlate = "   ";

        // Act
        var act = () => new Plate(whitespacePlate);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData("1234ABC")] 
    [InlineData("AB12345")] 
    [InlineData("ABCD123")] 
    [InlineData("ABC123")]  
    [InlineData("ABC12345")]
    [InlineData("ABC-1234")]
    [InlineData("ABC 1234")]
    [InlineData("ABC1A234")]
    public void Constructor_WhenPlateFormatIsInvalid_ShouldThrowDomainException(string invalidPlate)
    {
        // Arrange - já feito via InlineData

        // Act
        var act = () => new Plate(invalidPlate);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ToString_ShouldReturnPlateValue()
    {
        // Arrange
        var plate = new Plate("XYZ9876");

        // Act
        var result = plate.ToString();

        // Assert
        Assert.Equal("XYZ9876", result);
    }
}
