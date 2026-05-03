using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Controllers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.DTOs;
using OficinaApi.Tests.UnitTests.Application.DTOs;

namespace Unit.Tests;

public class VehicleControllerTests
{
    private readonly Mock<IVehicleService> _serviceMock;
    private readonly VehicleController _controller;

    public VehicleControllerTests()
    {
        _serviceMock = new Mock<IVehicleService>();
        _controller = new VehicleController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllVehicles()
    {
        _serviceMock.Setup(s => s.GetAllVehiclesAsync())
            .ReturnsAsync(new List<VehicleDto>());

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById()
    {
        var dto = VehicleDtoTests.CreateValid();

        _serviceMock.Setup(s => s.GetVehicleByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdIfExists()
    {
        _serviceMock.Setup(s => s.GetVehicleByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((VehicleDto)null);

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateVehicle()
    {
        var dto = VehicleDtoTests.CreateDto();
        var created = VehicleDtoTests.CreateValid();

        _serviceMock.Setup(s => s.CreateVehicleAsync(dto))
            .ReturnsAsync(created);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateVehicle()
    {
        var dto = VehicleDtoTests.UpdateDto();
        var updated = VehicleDtoTests.CreateValid();

        _serviceMock.Setup(s => s.UpdateVehicleAsync(It.IsAny<Guid>(), dto))
            .ReturnsAsync(updated);

        var result = await _controller.Update(Guid.NewGuid(), dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteVehicle()
    {
        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<OkResult>();
    }
}