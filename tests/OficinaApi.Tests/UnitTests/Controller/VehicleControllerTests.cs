using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Presentation.Controllers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.DTOs;
using OficinaApi.Tests.UnitTests.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unit.Tests;

public class VehicleControllerTests
{
    private readonly Mock<IUseCase<NoInput, IEnumerable<VehicleDto?>>> _getAllMock;
    private readonly Mock<IUseCase<Guid, VehicleDto?>> _getByIdMock;
    private readonly Mock<IUseCase<CreateVehicleDto, VehicleDto>> _createMock;
    private readonly Mock<IUseCase<UpdateVehicleRequest, VehicleDto>> _updateMock;
    private readonly Mock<IUseCase<Guid, bool>> _deleteMock;
    private readonly Mock<IUseCase<Guid, NoInput>> _logicalDeletion;
    private readonly VehicleController _controller;

    public VehicleControllerTests()
    {
        _getAllMock = new Mock<IUseCase<NoInput, IEnumerable<VehicleDto?>>>();
        _getByIdMock = new Mock<IUseCase<Guid, VehicleDto?>>();
        _createMock = new Mock<IUseCase<CreateVehicleDto, VehicleDto>>();
        _updateMock = new Mock<IUseCase<UpdateVehicleRequest, VehicleDto>>();
        _deleteMock = new Mock<IUseCase<Guid, bool>>();
        _logicalDeletion = new Mock<IUseCase<Guid, NoInput>>();

        _controller = new VehicleController(
            _getAllMock.Object,
            _getByIdMock.Object,
            _createMock.Object,
            _updateMock.Object,
            _deleteMock.Object,
            _logicalDeletion.Object);
    }

    [Fact]
    public async Task GetAllVehicles()
    {
        _getAllMock.Setup(s => s.ExecuteAsync(It.IsAny<NoInput>()))
            .ReturnsAsync(UseCaseResponse<IEnumerable<VehicleDto?>>.Success(new List<VehicleDto?>()));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById()
    {
        var dto = VehicleDtoTests.CreateValid();

        _getByIdMock.Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<VehicleDto?>.Success(dto));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdIfExists()
    {
        _getByIdMock.Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<VehicleDto?>.Success(null));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateVehicle()
    {
        var dto = VehicleDtoTests.CreateDto();
        var created = VehicleDtoTests.CreateValid();

        _createMock.Setup(s => s.ExecuteAsync(dto))
            .ReturnsAsync(UseCaseResponse<VehicleDto>.Success(created));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateVehicle()
    {
        var dto = VehicleDtoTests.UpdateDto();
        var updated = VehicleDtoTests.CreateValid();

        _updateMock.Setup(s => s.ExecuteAsync(It.IsAny<UpdateVehicleRequest>()))
            .ReturnsAsync(UseCaseResponse<VehicleDto>.Success(updated));

        var result = await _controller.Update(Guid.NewGuid(), dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteVehicle()
    {
        _deleteMock.Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<bool>.Success(true));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<OkResult>();
    }
}