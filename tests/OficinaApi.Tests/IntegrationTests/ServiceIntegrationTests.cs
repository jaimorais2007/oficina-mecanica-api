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

public class ServiceIntegrationTests
{
    private OficinaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dispatcherMock = new Mock<IDomainEventDispatcher>();

        return new OficinaDbContext(options, dispatcherMock.Object);
    }

    private ServiceManagementService CreateService(OficinaDbContext context)
    {
        var repo = new ServiceRepository(context);
        return new ServiceManagementService(repo);
    }

    #region Data Mocks

    private Service MockService()
    {
        return new Service(
            "Troca de Óleo",
            "Troca de óleo do motor",
            150m
        );
    }

    private CreateServiceDto MockCreateDto()
    {
        return new CreateServiceDto
        {
            Name = "Alinhamento",
            Description = "Alinhamento de rodas",
            DefaultPrice = 80m
        };
    }

    #endregion

    [Fact]
    public async Task CreateServiceIntegration()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var dto = MockCreateDto();

        var result = await service.CreateServiceAsync(dto);

        result.Should().NotBeNull();

        var entity = await context.Services.FirstOrDefaultAsync();

        entity.Should().NotBeNull();
        entity!.Name.Should().Be("Alinhamento");
    }

    [Fact]
    public async Task GetById()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var result = await service.GetServiceByIdAsync(entity.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var result = await service.GetAllServicesAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateService()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var dto = new UpdateServiceDto
        {
            Name = "Troca de Óleo Sintético",
            Description = "Atualizado",
            DefaultPrice = 200m
        };

        var result = await service.UpdateServiceAsync(entity.Id, dto);

        result.Should().NotBeNull();

        var updated = await context.Services.FirstAsync();

        updated.Name.Should().Be("Troca de Óleo Sintético");
        updated.DefaultPrice.Should().Be(200m);
    }

    [Fact]
    public async Task DeleteService()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        await service.DeleteServiceAsync(entity.Id);

        var exists = await context.Services.AnyAsync();

        exists.Should().BeFalse();
    }
}