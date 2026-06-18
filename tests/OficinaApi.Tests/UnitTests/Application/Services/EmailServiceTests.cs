using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Interfaces;

namespace Unit.Tests;

public class EmailServiceTests
{
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _sut = new EmailService();
    }

    [Fact]
    public void EmailService_ImplementaInterface()
    {
        _sut.Should().BeAssignableTo<IEmailService>();
    }

    [Fact]
    public async Task SendAsync_EmailDestinatarioInvalido_LancaExcecao()
    {
        // Endereço sem '@' é inválido e deve lançar exceção antes de conectar ao SMTP.
        var act = async () => await _sut.SendAsync(
            "nao-e-um-email",
            "Assunto de Teste",
            "Corpo do e-mail de teste."
        );

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendAsync_EmailVazio_LancaExcecao()
    {
        var act = async () => await _sut.SendAsync(
            string.Empty,
            "Assunto",
            "Corpo"
        );

        await act.Should().ThrowAsync<Exception>();
    }
}
