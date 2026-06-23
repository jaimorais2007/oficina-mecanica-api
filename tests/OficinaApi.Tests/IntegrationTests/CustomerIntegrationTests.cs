using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Customers;
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

public class CustomerIntegrationTests
{
    private OficinaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dispatcherMock = new Mock<IDomainEventDispatcher>();

        return new OficinaDbContext(options, dispatcherMock.Object);
    }

    #region Data mock
    private Customer MockCreateCustomer()
    {
        return new Customer(
            "Maria",
            PersonType.Individual,
            "75239335052",
            new DateTime(1995, 1, 1),
            "teste@gmail.com"
        );
    }
    
    private CreateCustomerDto MockCreateCustomerDto()
    {
        return new CreateCustomerDto
        {
            Name = "João",
            PersonType = PersonType.Individual,
            Document = "72119985049",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "teste@gmail.com"
        }; 
    }
    #endregion

    [Fact]
    public async Task CreateCustomer()
    {
        var context = CreateContext();
        var repo = new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>());
        var useCase = new CreateCustomerUseCase(repo);

        var dto = MockCreateCustomerDto();

        var result = await useCase.ExecuteAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var customer = await context.Customers.FirstOrDefaultAsync();

        customer.Should().NotBeNull();
        customer!.Name.Should().Be("João");
    }

    [Fact]
    public async Task GetById()
    {
        var context = CreateContext();
        var repo = new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>());
        var useCase = new GetCustomerByIdUseCase(repo, Mock.Of<ILogger<GetCustomerByIdUseCase>>());

        var customer = new Customer(
            "Maria",
            PersonType.Individual,
            "75239335052",
            new DateTime(1995, 1, 1),
            "teste@gmail.com"
        );

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(customer.Id);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var context = CreateContext();
        var repo = new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>());
        var useCase = new GetAllCustomersUseCase(repo);

        var customer = MockCreateCustomer();

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(new NoInput());

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateCustomer()
    {
        var context = CreateContext();
        var repo = new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>());
        var useCase = new UpdateCustomerUseCase(repo, Mock.Of<ILogger<UpdateCustomerUseCase>>());

        var customer = MockCreateCustomer();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var dto = new UpdateCustomerDto
        {
            Name = "Novo Nome",
            PersonType = PersonType.Individual,
            Document = "75239335052",
            DateOfBirth = new DateTime(2000, 1, 1),
            Email = "teste@gmail.com"
        };

        var result = await useCase.ExecuteAsync(new UpdateCustomerRequest(customer.Id, dto));

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();

        var updated = await context.Customers.FirstAsync();

        updated.Name.Should().Be("Novo Nome");
    }

    [Fact]
    public async Task DeleteCustomer()
    {
        var context = CreateContext();
        var repo = new CustomerRepository(context, Mock.Of<ILogger<CustomerRepository>>());
        var useCase = new DeleteCustomerUseCase(repo, Mock.Of<ILogger<DeleteCustomerUseCase>>());

        var customer = MockCreateCustomer();

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = await useCase.ExecuteAsync(customer.Id);
        result.IsSuccess.Should().BeTrue();

        var exists = await context.Customers.AnyAsync();

        exists.Should().BeFalse();
    }
}