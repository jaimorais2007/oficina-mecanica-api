using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using Xunit;

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

    private async Task<(VehicleService service, Customer customer, OficinaDbContext context)> Setup()
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

        var vehicleRepo = new VehicleRepository(context);
        var customerRepo = new CustomerRepository(context);
        var service = new VehicleService(vehicleRepo, customerRepo);

        return (service, customer, context);
    }

    [Fact]
    public async Task CreateVehicle()
    {
        var (service, customer, context) = await Setup();

        var dto = new CreateVehicleDto
        {
            CustomerId = customer.Id,
            Plate = "ABC1234",
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2020
        };

        var result = await service.CreateVehicleAsync(dto);

        result.Should().NotBeNull();

        var vehicle = await context.Vehicles.FirstOrDefaultAsync();
        vehicle.Should().NotBeNull();
        vehicle!.Plate.Value.Should().Be("ABC1234");
    }

    [Fact]
    public async Task GetById()
    {
        var (service, customer, context) = await Setup();

        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var result = await service.GetVehicleByIdAsync(vehicle.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(vehicle.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var (service, customer, context) = await Setup();

        context.Vehicles.Add(new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020));
        context.Vehicles.Add(new Vehicle(customer, "DEF5678", "Honda", "Civic", 2021));

        await context.SaveChangesAsync();

        var result = await service.GetAllVehiclesAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateVehicle()
    {
        var (service, customer, context) = await Setup();

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

        var result = await service.UpdateVehicleAsync(vehicle.Id, dto);

        result.Should().NotBeNull();

        var updated = await context.Vehicles.FirstAsync();

        updated.Plate.Value.Should().Be("XYZ9999");
        updated.Brand.Should().Be("Ford");
    }

    [Fact]
    public async Task DeleteVehicle()
    {
        var (service, customer, context) = await Setup();

        var vehicle = new Vehicle(customer, "ABC1234", "Toyota", "Corolla", 2020);

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        await service.DeleteVehicleAsync(vehicle.Id);

        var exists = await context.Vehicles.AnyAsync();

        exists.Should().BeFalse();
    }
}