using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Presentation.Controllers;
using Xunit;

namespace Unit.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUseCase<AuthenticateUserRequest, UserDto?>> _authMock;
    private readonly Mock<IUseCase<GenerateTokenRequest, string>> _tokenMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authMock = new Mock<IUseCase<AuthenticateUserRequest, UserDto?>>();
        _tokenMock = new Mock<IUseCase<GenerateTokenRequest, string>>();
        _controller = new AuthController(_authMock.Object, _tokenMock.Object);
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
        _authMock
            .Setup(s => s.ExecuteAsync(It.IsAny<AuthenticateUserRequest>()))
            .ReturnsAsync(UseCaseResponse<UserDto?>.Success(null));

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

        _authMock
            .Setup(s => s.ExecuteAsync(It.Is<AuthenticateUserRequest>(r => r.Email == "admin@oficina.com" && r.Password == "senha123")))
            .ReturnsAsync(UseCaseResponse<UserDto?>.Success(userDto));

        _tokenMock
            .Setup(s => s.ExecuteAsync(It.IsAny<GenerateTokenRequest>()))
            .ReturnsAsync(UseCaseResponse<string>.Success("token-mock"));

        var dto = new LoginDto { Email = "admin@oficina.com", Password = "senha123" };

        var result = await _controller.Login(dto);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }
}
