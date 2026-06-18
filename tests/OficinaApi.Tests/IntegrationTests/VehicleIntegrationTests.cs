using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Integration.Tests;

public class VehicleIntegrationTests
{
    private OficinaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dispatcherMock = new Mock<IDomainEventDispatcher>();

        return new OficinaDbContext(options, dispatcherMock.Object);
    }

    private async Task<(Customer customer, OficinaDbContext context)> Setup()
    {
        var context = CreateContext();

        var customer = new Customer(
            "João",
            PersonType.Individual,
            "72119985049",
            new DateTime(1990, 1, 1),
            "teste@gmail.com"
        );

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return (customer, context);
    }

    [Fact]
    public async Task CreateVehicle()
    {
        var (customer, context) = await Setup();
        var vehicleRepo = new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>());
        var customerRepo = new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>());
        var useCase = new CreateVehicleUseCase(vehicleRepo, customerRepo, Mock.Of<ILogger<CreateVehicleUseCase>>());

        var dto = new CreateVehicleDto
        {
            CustomerId = customer.Id,
            Plate = "ABC1234",
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2020
        };

        var result = await useCase.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var vehicle = await context.Vehicles.FirstOrDefaultAsync();
        vehicle.Should().NotBeNull();
        vehicle!.Plate.Value.Should().Be("ABC1234");
    }

    [Fact]
    public async Task GetById()
    {
        var (customer, context) = await Setup();
        var vehicleRepo = new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>());
        var useCase = new GetVehicleByIdUseCase(vehicleRepo, Mock.Of<ILogger<GetVehicleByIdUseCase>>());

        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(vehicle.Id);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response!.Id.Should().Be(vehicle.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var (customer, context) = await Setup();
        var vehicleRepo = new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>());
        var useCase = new GetAllVehiclesUseCase(vehicleRepo);

        context.Vehicles.Add(new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020));
        context.Vehicles.Add(new Vehicle(customer, "DEF5678", "Honda", "Civic", 2021));

        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new NoInput());

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateVehicle()
    {
        var (customer, context) = await Setup();
        var vehicleRepo = new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>());
        var useCase = new UpdateVehicleUseCase(vehicleRepo, Mock.Of<ILogger<UpdateVehicleUseCase>>());

        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var dto = new UpdateVehicleDto
        {
            Plate = "XYZ9999",
            Brand = "Ford",
            Model = "Focus",
            Year = 2022
        };

        var result = await useCase.ExecuteAsync(new UpdateVehicleRequest(vehicle.Id, dto));

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var updated = await context.Vehicles.FirstAsync();

        updated.Plate.Value.Should().Be("XYZ9999");
        updated.Brand.Should().Be("Ford");
    }

    [Fact]
    public async Task DeleteVehicle()
    {
        var (customer, context) = await Setup();
        var vehicleRepo = new VehicleRepository(context, Mock.Of<ILogger<VehicleRepository>>());
        var useCase = new DeleteVehicleUseCase(vehicleRepo, Mock.Of<ILogger<DeleteVehicleUseCase>>());

        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(vehicle.Id);
        result.IsSuccess.Should().BeTrue();

        var exists = await context.Vehicles.AnyAsync();

        exists.Should().BeFalse();
    }
}