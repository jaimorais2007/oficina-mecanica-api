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

    private CustomerService CreateService(OficinaDbContext context)
    {
        var repo = new CustomerRepository(context);
        return new CustomerService(repo);
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
        var service = CreateService(context);

        var dto = MockCreateCustomerDto();

        var result = await service.CreateCustomerAsync(dto);

        result.Should().NotBeNull();

        var customer = await context.Customers.FirstOrDefaultAsync();

        customer.Should().NotBeNull();
        customer!.Name.Should().Be("João");
    }

    [Fact]
    public async Task GetById()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var customer = new Customer(
            "Maria",
            PersonType.Individual,
            "75239335052",
            new DateTime(1995, 1, 1),
            "teste@gmail.com"
        );

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = await service.GetCustomerByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task GetAll()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var customer = MockCreateCustomer();

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var result = await service.GetAllCustomersAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateCustomer()
    {
        var context = CreateContext();
        var service = CreateService(context);

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

        var result = await service.UpdateCustomerAsync(customer.Id, dto);

        result.Should().NotBeNull();

        var updated = await context.Customers.FirstAsync();

        updated.Name.Should().Be("Novo Nome");
    }

    [Fact]
    public async Task DeleteCustomer()
    {
        var context = CreateContext();
        var service = CreateService(context);

        var customer = MockCreateCustomer();

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        await service.DeleteCustomerAsync(customer.Id);

        var exists = await context.Customers.AnyAsync();

        exists.Should().BeFalse();
    }
}