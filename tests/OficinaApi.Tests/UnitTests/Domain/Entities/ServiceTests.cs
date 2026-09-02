using FluentAssertions;
using Xunit;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Exceptions;

namespace Unit.Tests;

public class ServiceTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetAllProperties()
    {
        var name = "Troca de Óleo";
        var description = "Troca de óleo do motor";
        var price = 150.00m;
        var before = DateTime.UtcNow;

        var service = new Service(name, description, price);

        service.Id.Should().NotBeEmpty();
        service.Name.Should().Be(name);
        service.Description.Should().Be(description);
        service.DefaultPrice.Should().Be(price);
        service.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeEmptyServiceOrdersServices()
    {
        var service = new Service("Alinhamento", "Alinhamento de rodas", 80m);

        service.ServiceOrdersServices.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldNotThrow()
    {
        var act = () => new Service("Revisão", "Revisão básica", 0m);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowDomainException(string? invalidName)
    {
        var act = () => new Service(invalidName!, "Descrição", 100m);

        act.Should().Throw<DomainException>()
            .WithMessage("*Nome do serviço é obrigatório*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowDomainException()
    {
        var act = () => new Service("Balanceamento", "Balanceamento de rodas", -1m);

        act.Should().Throw<DomainException>()
            .WithMessage("*Preço informado é inválido*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        var service = new Service("Troca de Óleo", "Descrição original", 100m);

        service.Update("Troca de Óleo Sintético", "Descrição atualizada", 200m);

        service.Name.Should().Be("Troca de Óleo Sintético");
        service.Description.Should().Be("Descrição atualizada");
        service.DefaultPrice.Should().Be(200m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_WithInvalidName_ShouldThrowDomainException(string? invalidName)
    {
        var service = new Service("Troca de Óleo", "Descrição", 100m);

        var act = () => service.Update(invalidName!, "Descrição", 100m);

        act.Should().Throw<DomainException>()
            .WithMessage("*Nome do serviço é obrigatório*");
    }

    [Fact]
    public void Update_WithNegativePrice_ShouldThrowDomainException()
    {
        var service = new Service("Troca de Óleo", "Descrição", 100m);

        var act = () => service.Update("Troca de Óleo", "Descrição", -50m);

        act.Should().Throw<DomainException>()
            .WithMessage("*Preço informado é inválido*");
    }

}
