using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.WebApi.Controllers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.DTOs;
using OficinaApi.Tests.UnitTests.Application.DTOs;
namespace Unit.Tests;

public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _serviceMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _serviceMock = new Mock<ICustomerService>();
        _controller = new CustomerController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllCustomers()
    {
        _serviceMock.Setup(s => s.GetAllCustomersAsync())
            .ReturnsAsync(new List<CustomerDto>());

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdIfExists()
    {
        var dto = CustomerDtoTests.CreateValid();

        _serviceMock.Setup(s => s.GetCustomerByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById()
    {
        _serviceMock.Setup(s => s.GetCustomerByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((CustomerDto)null);

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateCustomer()
    {
        var dto = CustomerDtoTests.CreateDto();
        var created = CustomerDtoTests.CreateValid();

        _serviceMock.Setup(s => s.CreateCustomerAsync(dto))
            .ReturnsAsync(created);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateCustomer()
    {
        var dto = CustomerDtoTests.UpdateDto();
        var updated = CustomerDtoTests.CreateValid();

        _serviceMock.Setup(s => s.UpdateCustomerAsync(It.IsAny<Guid>(), dto))
            .ReturnsAsync(updated);

        var result = await _controller.Update(Guid.NewGuid(), dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteCustomer()
    {
        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }
}