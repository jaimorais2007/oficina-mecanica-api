using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using OficinaApi.WebApi.Controllers;
using Xunit;

namespace Unit.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly TokenService _tokenService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]           = "chave-de-teste-minimo-32-caracteres!!",
                ["Jwt:Issuer"]           = "oficina-api",
                ["Jwt:Audience"]         = "oficina-clientes",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        _tokenService = new TokenService(config);
        _controller   = new AuthController(_tokenService, _userServiceMock.Object);
    }

    [Fact]
    public async Task Login_EmailVazio_RetornaBadRequest()
    {
        var dto = new LoginDto { Email = "", Password = "senha123" };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_SenhaVazia_RetornaBadRequest()
    {
        var dto = new LoginDto { Email = "user@email.com", Password = "" };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_CredenciaisInvalidas_RetornaUnauthorized()
    {
        _userServiceMock
            .Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((UserDto?)null);

        var dto = new LoginDto { Email = "user@email.com", Password = "senha_errada" };

        var result = await _controller.Login(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaOkComToken()
    {
        var userDto = new UserDto
        {
            Id    = Guid.NewGuid(),
            Name  = "Admin",
            Email = "admin@oficina.com",
            Role  = "Admin"
        };

        _userServiceMock
            .Setup(s => s.AuthenticateAsync("admin@oficina.com", "senha123"))
            .ReturnsAsync(userDto);

        var dto = new LoginDto { Email = "admin@oficina.com", Password = "senha123" };

        var result = await _controller.Login(dto);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }
}
