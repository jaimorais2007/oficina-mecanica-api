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

public class UsersControllerTests
{
    private readonly Mock<IUseCase<NoInput, IEnumerable<UserDto>>> _getAllMock;
    private readonly Mock<IUseCase<Guid, UserDto?>> _getByIdMock;
    private readonly Mock<IUseCase<CreateUserDto, UserDto>> _createMock;
    private readonly Mock<IUseCase<UpdateUserRequest, bool>> _updateMock;
    private readonly Mock<IUseCase<Guid, bool>> _deleteMock;
    private readonly Mock<IUseCase<Guid, NoInput>> _logicalDeletion;

    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _getAllMock = new Mock<IUseCase<NoInput, IEnumerable<UserDto>>>();
        _getByIdMock = new Mock<IUseCase<Guid, UserDto?>>();
        _createMock = new Mock<IUseCase<CreateUserDto, UserDto>>();
        _updateMock = new Mock<IUseCase<UpdateUserRequest, bool>>();
        _deleteMock = new Mock<IUseCase<Guid, bool>>();
        _logicalDeletion = new Mock<IUseCase<Guid, NoInput>>();


        _controller = new UsersController(
            _getAllMock.Object,
            _getByIdMock.Object,
            _createMock.Object,
            _updateMock.Object,
            _deleteMock.Object,
            _logicalDeletion.Object);
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
        _getAllMock
            .Setup(s => s.ExecuteAsync(It.IsAny<NoInput>()))
            .ReturnsAsync(UseCaseResponse<IEnumerable<UserDto>>.Success(new List<UserDto> { BuildUserDto() }));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_UsuarioExistente_RetornaOk()
    {
        var dto = BuildUserDto();
        _getByIdMock
            .Setup(s => s.ExecuteAsync(dto.Id))
            .ReturnsAsync(UseCaseResponse<UserDto?>.Success(dto));

        var result = await _controller.GetById(dto.Id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_UsuarioInexistente_RetornaNotFound()
    {
        _getByIdMock
            .Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<UserDto?>.Success(null));

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

        _createMock
            .Setup(s => s.ExecuteAsync(createDto))
            .ReturnsAsync(UseCaseResponse<UserDto>.Success(created));

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

        _createMock
            .Setup(s => s.ExecuteAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(UseCaseResponse<UserDto>.Failure("E-mail já cadastrado."));

        var result = await _controller.Create(createDto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_DadosValidos_RetornaNoContent()
    {
        var updateDto = new UpdateUserDto { Name = "Novo Nome", Role = "User" };

        _updateMock
            .Setup(s => s.ExecuteAsync(It.IsAny<UpdateUserRequest>()))
            .ReturnsAsync(UseCaseResponse<bool>.Success(true));

        var result = await _controller.Update(Guid.NewGuid(), updateDto);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_UsuarioInexistente_RetornaBadRequest()
    {
        var updateDto = new UpdateUserDto { Name = "Nome", Role = "User" };

        _updateMock
            .Setup(s => s.ExecuteAsync(It.IsAny<UpdateUserRequest>()))
            .ReturnsAsync(UseCaseResponse<bool>.Failure("Usuário não encontrado."));

        var result = await _controller.Update(Guid.NewGuid(), updateDto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_RetornaNoContent()
    {
        _deleteMock
            .Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<bool>.Success(true));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }
}
