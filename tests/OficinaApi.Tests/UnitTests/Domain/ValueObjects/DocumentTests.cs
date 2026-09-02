using FluentAssertions;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Exceptions;
using OficinaApi.Domain.ValueObjects;
using Xunit;

namespace Unit.Tests;

public class DocumentTests
{
    private const string ValidCpf = "11144477735";
    private const string ValidCpfFormatted = "111.444.777-35";

    private const string ValidCnpj = "11222333000181";
    private const string ValidCnpjFormatted = "11.222.333/0001-81";

    [Fact]
    public void Constructor_WhenValidCpfWithoutMask_ShouldCreateDocument()
    {
        var rawCpf = ValidCpf;
        var document = new Document(rawCpf, PersonType.Individual);

        document.Value.Should().Be(ValidCpf);
    }

    [Fact]
    public void Constructor_WhenValidCpfWithMask_ShouldStripMaskAndCreateDocument()
    {
        // Arrange
        var formattedCpf = ValidCpfFormatted;

        // Act
        var document = new Document(formattedCpf, PersonType.Individual);

        // Assert
        document.Value.Should().Be(ValidCpf);
    }

    [Fact]
    public void Constructor_WhenInvalidCpfWrongDigit_ShouldThrowDomainException()
    {
        // Arrange
        var invalidCpf = "11144477736"; // último dígito incorreto

        // Act
        var act = () => new Document(invalidCpf, PersonType.Individual);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("CPF inválido.");
    }

    [Fact]
    public void Constructor_WhenCpfWithAllSameDigits_ShouldThrowDomainException()
    {
        // Arrange
        var allSameCpf = "11111111111";

        // Act
        var act = () => new Document(allSameCpf, PersonType.Individual);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("CPF inválido.");
    }

    [Fact]
    public void Constructor_WhenCpfWithWrongLength_ShouldThrowDomainException()
    {
        // Arrange
        var shortCpf = "1234567890"; // 10 dígitos

        // Act
        var act = () => new Document(shortCpf, PersonType.Individual);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("CPF inválido.");
    }


    [Fact]
    public void Constructor_WhenValidCnpjWithoutMask_ShouldCreateDocument()
    {
        // Arrange
        var rawCnpj = ValidCnpj;

        // Act
        var document = new Document(rawCnpj, PersonType.Company);

        // Assert
        document.Value.Should().Be(ValidCnpj);
    }

    [Fact]
    public void Constructor_WhenValidCnpjWithMask_ShouldStripMaskAndCreateDocument()
    {
        // Arrange
        var formattedCnpj = ValidCnpjFormatted;

        // Act
        var document = new Document(formattedCnpj, PersonType.Company);

        // Assert
        document.Value.Should().Be(ValidCnpj);
    }

    [Fact]
    public void Constructor_WhenInvalidCnpjWrongDigit_ShouldThrowDomainException()
    {
        // Arrange
        var invalidCnpj = "11222333000182"; // último dígito incorreto

        // Act
        var act = () => new Document(invalidCnpj, PersonType.Company);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("CNPJ inválido.");
    }

    [Fact]
    public void Constructor_WhenCnpjWithAllSameDigits_ShouldThrowDomainException()
    {
        // Arrange
        var allSameCnpj = "11111111111111";

        // Act
        var act = () => new Document(allSameCnpj, PersonType.Company);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("CNPJ inválido.");
    }

    [Fact]
    public void Constructor_WhenCnpjWithWrongLength_ShouldThrowDomainException()
    {
        // Arrange
        var shortCnpj = "1122233300018"; // 13 dígitos

        // Act
        var act = () => new Document(shortCnpj, PersonType.Company);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("CNPJ inválido.");
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenDocumentIsNullOrWhiteSpace_ShouldThrowDomainException(string? emptyDocument)
    {
        // Arrange & Act
        var act = () => new Document(emptyDocument!, PersonType.Individual);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Documento é obrigatório.");
    }

    [Fact]
    public void Constructor_WhenPersonTypeIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var invalidPersonType = (PersonType)99;

        // Act
        var act = () => new Document(ValidCpf, invalidPersonType);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Tipo de pessoa inválido.");
    }


    [Fact]
    public void ToString_WhenDocumentIsValid_ShouldReturnValue()
    {
        // Arrange
        var document = new Document(ValidCpf, PersonType.Individual);

        // Act
        var result = document.ToString();

        // Assert
        result.Should().Be(ValidCpf);
    }

}
