using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Parts;
using OficinaApi.Domain.Entities;
using OficinaApi.Infrastructure.Data;
using OficinaApi.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

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
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new CreatePartUseCase(repo);

        var dto = CreateDto();

        var result = await useCase.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var entity = await context.Parts.FirstOrDefaultAsync();

        entity.Should().NotBeNull();
        entity!.Name.Should().Be("Filtro de óleo");
        entity.QuantityInStock.Should().Be(10);
    }

    [Fact]
    public async Task GetById()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new GetPartByIdUseCase(repo, Mock.Of<ILogger<GetPartByIdUseCase>>());

        var part = new Part("Filtro", "F1", 5, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(part.Id);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response!.Id.Should().Be(part.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new GetAllPartsUseCase(repo);

        context.Parts.Add(new Part("P1", "C1", 5, 10m));
        context.Parts.Add(new Part("P2", "C2", 10, 20m));
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new NoInput());

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddStock()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new AddStockUseCase(repo, Mock.Of<ILogger<AddStockUseCase>>());

        var part = new Part("Filtro", "F1", 10, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new AddStockRequest(part.Id, 5));
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Parts.FirstAsync();

        updated.QuantityInStock.Should().Be(15);
    }

    [Fact]
    public async Task RemoveStock()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new RemoveStockUseCase(repo, Mock.Of<ILogger<RemoveStockUseCase>>());

        var part = new Part("Filtro", "F1", 10, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new RemoveStockRequest(part.Id, 3));
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Parts.FirstAsync();

        updated.QuantityInStock.Should().Be(7);
    }

    [Fact]
    public async Task RemoveStockWhenInsufficientStock()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new RemoveStockUseCase(repo, Mock.Of<ILogger<RemoveStockUseCase>>());

        var part = new Part("Filtro", "F1", 2, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new RemoveStockRequest(part.Id, 5));

        result.IsSuccess.Should().BeFalse();
        result.Messages.Should().ContainMatch("*Estoque insuficiente*");
    }

    [Fact]
    public async Task AddStockWhenPartNotFound()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new AddStockUseCase(repo, Mock.Of<ILogger<AddStockUseCase>>());

        var result = await useCase.ExecuteAsync(new AddStockRequest(Guid.NewGuid(), 5));

        result.IsSuccess.Should().BeFalse();
        result.Messages.Should().ContainMatch("*não encontrada*");
    }

    [Fact]
    public async Task Delete()
    {
        var context = CreateContext();
        var repo = new PartRepository(context, Mock.Of<ILogger<PartRepository>>());
        var useCase = new DeletePartUseCase(repo, Mock.Of<ILogger<DeletePartUseCase>>());

        var part = new Part("Filtro", "F1", 10, 30m);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(part.Id);
        result.IsSuccess.Should().BeTrue();

        var exists = await context.Parts.AnyAsync();

        exists.Should().BeFalse();
    }
}