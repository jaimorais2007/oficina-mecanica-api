using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Interfaces;
using Xunit;

namespace Unit.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _sut      = new UserService(_repoMock.Object);
    }

    private static User BuildUser(string email = "user@oficina.com")
    {
        string hash = BCrypt.Net.BCrypt.HashPassword("senha123");
        return new User("João Silva", email, hash, "Admin");
    }

    [Fact]
    public async Task GetAllUsersAsync_RetornaListaDtos()
    {
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { BuildUser() });

        var result = await _sut.GetAllUsersAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserByIdAsync_UsuarioExistente_RetornaDto()
    {
        var user = BuildUser();
        _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _sut.GetUserByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_UsuarioInexistente_RetornaNull()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_EmailNovo_CriaERetornaDto()
    {
        _repoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var dto = new CreateUserDto
        {
            Name     = "Maria",
            Email    = "maria@oficina.com",
            Password = "senha123",
            Role     = "User"
        };

        var result = await _sut.CreateUserAsync(dto);

        result.Should().NotBeNull();
        result.Email.Should().Be("maria@oficina.com");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_EmailDuplicado_LancaExcecao()
    {
        var existingUser = BuildUser("duplicado@oficina.com");
        _repoMock
            .Setup(r => r.GetByEmailAsync("duplicado@oficina.com"))
            .ReturnsAsync(existingUser);

        var dto = new CreateUserDto
        {
            Name     = "Clone",
            Email    = "duplicado@oficina.com",
            Password = "senha123",
            Role     = "User"
        };

        await Assert.ThrowsAsync<Exception>(() => _sut.CreateUserAsync(dto));
    }

    [Fact]
    public async Task UpdateUserAsync_UsuarioExistente_AtualizaEChama()
    {
        var user = BuildUser();
        _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _repoMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);

        var dto = new UpdateUserDto { Name = "Novo Nome", Role = "User" };

        await _sut.UpdateUserAsync(user.Id, dto);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_UsuarioInexistente_LancaExcecao()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        var dto = new UpdateUserDto { Name = "Nome", Role = "User" };

        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateUserAsync(Guid.NewGuid(), dto));
    }

    [Fact]
    public async Task DeleteUserAsync_ChamaRepositorio()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        await _sut.DeleteUserAsync(id);

        _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_CredenciaisValidas_RetornaDto()
    {
        var user = BuildUser("auth@oficina.com");
        _repoMock.Setup(r => r.GetByEmailAsync("auth@oficina.com")).ReturnsAsync(user);

        var result = await _sut.AuthenticateAsync("auth@oficina.com", "senha123");

        result.Should().NotBeNull();
        result!.Email.Should().Be("auth@oficina.com");
    }

    [Fact]
    public async Task AuthenticateAsync_SenhaErrada_RetornaNull()
    {
        var user = BuildUser("auth@oficina.com");
        _repoMock.Setup(r => r.GetByEmailAsync("auth@oficina.com")).ReturnsAsync(user);

        var result = await _sut.AuthenticateAsync("auth@oficina.com", "senha_errada");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_UsuarioInexistente_RetornaNull()
    {
        _repoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.AuthenticateAsync("naoexiste@oficina.com", "senha123");

        result.Should().BeNull();
    }
}
