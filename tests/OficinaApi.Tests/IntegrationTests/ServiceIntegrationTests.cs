using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Services;
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
        var repo = new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>());
        var useCase = new CreateServiceUseCase(repo);

        var dto = MockCreateDto();

        var result = await useCase.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var entity = await context.Services.FirstOrDefaultAsync();

        entity.Should().NotBeNull();
        entity!.Name.Should().Be("Alinhamento");
    }

    [Fact]
    public async Task GetById()
    {
        var context = CreateContext();
        var repo = new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>());
        var useCase = new GetServiceByIdUseCase(repo, Mock.Of<ILogger<GetServiceByIdUseCase>>());

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(entity.Id);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var context = CreateContext();
        var repo = new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>());
        var useCase = new GetAllServicesUseCase(repo);

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new NoInput());

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateService()
    {
        var context = CreateContext();
        var repo = new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>());
        var useCase = new UpdateServiceUseCase(repo, Mock.Of<ILogger<UpdateServiceUseCase>>());

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var dto = new UpdateServiceDto
        {
            Name = "Troca de Óleo Sintético",
            Description = "Atualizado",
            DefaultPrice = 200m
        };

        var result = await useCase.ExecuteAsync(new UpdateServiceRequest(entity.Id, dto));

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var updated = await context.Services.FirstAsync();

        updated.Name.Should().Be("Troca de Óleo Sintético");
        updated.DefaultPrice.Should().Be(200m);
    }

    [Fact]
    public async Task DeleteService()
    {
        var context = CreateContext();
        var repo = new ServiceRepository(context, Mock.Of<ILogger<ServiceRepository>>());
        var useCase = new DeleteServiceUseCase(repo, Mock.Of<ILogger<DeleteServiceUseCase>>());

        var entity = MockService();

        context.Services.Add(entity);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(entity.Id);
        result.IsSuccess.Should().BeTrue();

        var exists = await context.Services.AnyAsync();

        exists.Should().BeFalse();
    }
}