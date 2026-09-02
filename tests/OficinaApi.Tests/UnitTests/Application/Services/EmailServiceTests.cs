using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Interfaces;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Unit.Tests;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();
        _sut = new EmailService(_configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void EmailService_ImplementaInterface()
    {
        _sut.Should().BeAssignableTo<IEmailService>();
    }

    [Fact]
    public async Task SendAsync_EmailDestinatarioInvalido_LancaExcecaoERegistraLogErro()
    {
        // Endereço sem '@' é inválido e deve lançar exceção antes de conectar ao SMTP.
        var act = async () => await _sut.SendAsync(
            "nao-e-um-email",
            "Assunto de Teste",
            "Corpo do e-mail de teste."
        );

        await act.Should().ThrowAsync<Exception>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_EmailVazio_LancaExcecaoERegistraLogErro()
    {
        var act = async () => await _sut.SendAsync(
            string.Empty,
            "Assunto",
            "Corpo"
        );

        await act.Should().ThrowAsync<Exception>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
