using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Controllers;
using Xunit;

namespace Unit.Tests;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _controller  = new UsersController(_serviceMock.Object);
    }

    private static UserDto BuildUserDto() => new()
    {
        Id        = Guid.NewGuid(),
        Name      = "João Silva",
        Email     = "joao@oficina.com",
        Role      = "Admin",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetAll_RetornaOkComLista()
    {
        _serviceMock
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<UserDto> { BuildUserDto() });

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_UsuarioExistente_RetornaOk()
    {
        var dto = BuildUserDto();
        _serviceMock
            .Setup(s => s.GetUserByIdAsync(dto.Id))
            .ReturnsAsync(dto);

        var result = await _controller.GetById(dto.Id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_UsuarioInexistente_RetornaNotFound()
    {
        _serviceMock
            .Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((UserDto?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_DadosValidos_RetornaCreated()
    {
        var createDto = new CreateUserDto
        {
            Name     = "Maria",
            Email    = "maria@oficina.com",
            Password = "senha123",
            Role     = "User"
        };
        var created = BuildUserDto();

        _serviceMock
            .Setup(s => s.CreateUserAsync(createDto))
            .ReturnsAsync(created);

        var result = await _controller.Create(createDto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_EmailDuplicado_RetornaBadRequest()
    {
        var createDto = new CreateUserDto
        {
            Name     = "Maria",
            Email    = "duplicado@oficina.com",
            Password = "senha123",
            Role     = "User"
        };

        _serviceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ThrowsAsync(new Exception("E-mail já cadastrado."));

        var result = await _controller.Create(createDto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_DadosValidos_RetornaNoContent()
    {
        var updateDto = new UpdateUserDto { Name = "Novo Nome", Role = "User" };

        _serviceMock
            .Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), updateDto))
            .Returns(Task.CompletedTask);

        var result = await _controller.Update(Guid.NewGuid(), updateDto);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_UsuarioInexistente_RetornaBadRequest()
    {
        var updateDto = new UpdateUserDto { Name = "Nome", Role = "User" };

        _serviceMock
            .Setup(s => s.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserDto>()))
            .ThrowsAsync(new Exception("Usuário não encontrado."));

        var result = await _controller.Update(Guid.NewGuid(), updateDto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_RetornaNoContent()
    {
        _serviceMock
            .Setup(s => s.DeleteUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }
}
