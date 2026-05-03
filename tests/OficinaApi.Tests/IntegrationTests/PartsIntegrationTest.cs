using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Entities;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using Xunit;

namespace Integration.Tests;

public class PartIntegrationTests
{
    private OficinaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dispatcherMock = new Mock<IDomainEventDispatcher>();

        return new OficinaDbContext(options, dispatcherMock.Object);
    }

    private PartService CreateService(OficinaDbContext context)
    {
        var repo = new PartRepository(context);
        return new PartService(repo);
    }

    #region Mocks

    private CreatePartDto CreateDto() => new()
    {
        Name = "Filtro de óleo",
        Code = "FO-001",
        InitialQuantity = 10,
        Price = 50m
    };

    #endregion

    [Fact]
    public async Task CreatePart()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var dto = CreateDto();

        var result = await service.CreatePartAsync(dto);

        result.Should().NotBeNull();

        var entity = await context.Parts.FirstOrDefaultAsync();

        entity.Should().NotBeNull();
        entity!.Name.Should().Be("Filtro de óleo");
        entity.QuantityInStock.Should().Be(10);
    }

    [Fact]
    public async Task GetById()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var part = new Part("Filtro", "F1", 5, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var result = await service.GetPartByIdAsync(part.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(part.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var context = CreateContext();
        var service = CreateService(context);

        context.Parts.Add(new Part("P1", "C1", 5, 10m));
        context.Parts.Add(new Part("P2", "C2", 10, 20m));
        await context.SaveChangesAsync();

        var result = await service.GetAllPartsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddStock()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var part = new Part("Filtro", "F1", 10, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        await service.AddStockAsync(part.Id, 5);

        var updated = await context.Parts.FirstAsync();

        updated.QuantityInStock.Should().Be(15);
    }

    [Fact]
    public async Task RemoveStock()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var part = new Part("Filtro", "F1", 10, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        await service.RemoveStockAsync(part.Id, 3);

        var updated = await context.Parts.FirstAsync();

        updated.QuantityInStock.Should().Be(7);
    }

    [Fact]
    public async Task RemoveStockWhenInsufficientStock()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var part = new Part("Filtro", "F1", 2, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        Func<Task> act = async () => await service.RemoveStockAsync(part.Id, 5);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddStockWhenPartNotFound()
    {
        var context = CreateContext();
        var service = CreateService(context);

        Func<Task> act = async () => await service.AddStockAsync(Guid.NewGuid(), 5);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Delete()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var part = new Part("Filtro", "F1", 10, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        await service.DeletePartAsync(part.Id);

        var exists = await context.Parts.AnyAsync();

        exists.Should().BeFalse();
    }
}