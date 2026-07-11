using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Presentation.Controllers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.DTOs;
using OficinaApi.Tests.UnitTests.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unit.Tests;

public class CustomerControllerTests
{
    private readonly Mock<IUseCase<NoInput, IEnumerable<CustomerDto>>> _getAllMock;
    private readonly Mock<IUseCase<Guid, CustomerDto?>> _getByIdMock;
    private readonly Mock<IUseCase<CreateCustomerDto, CustomerDto>> _createMock;
    private readonly Mock<IUseCase<UpdateCustomerRequest, CustomerDto>> _updateMock;
    private readonly Mock<IUseCase<Guid, bool>> _deleteMock;
    private readonly Mock<IUseCase<Guid, NoInput>> _logicalDeletion;

    private readonly CustomerController _controller;


    public CustomerControllerTests()
    {
        _getAllMock = new Mock<IUseCase<NoInput, IEnumerable<CustomerDto>>>();
        _getByIdMock = new Mock<IUseCase<Guid, CustomerDto?>>();
        _createMock = new Mock<IUseCase<CreateCustomerDto, CustomerDto>>();
        _updateMock = new Mock<IUseCase<UpdateCustomerRequest, CustomerDto>>();
        _deleteMock = new Mock<IUseCase<Guid, bool>>();
        _logicalDeletion = new Mock<IUseCase<Guid, NoInput>>();


        _controller = new CustomerController(
            _getAllMock.Object,
            _getByIdMock.Object,
            _createMock.Object,
            _updateMock.Object,
            _deleteMock.Object,
            _logicalDeletion.Object);
    }

    [Fact]
    public async Task GetAllCustomers()
    {
        _getAllMock.Setup(s => s.ExecuteAsync(It.IsAny<NoInput>()))
            .ReturnsAsync(UseCaseResponse<IEnumerable<CustomerDto>>.Success(new List<CustomerDto>()));

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdIfExists()
    {
        var dto = CustomerDtoTests.CreateValid();

        _getByIdMock.Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<CustomerDto?>.Success(dto));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById()
    {
        _getByIdMock.Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<CustomerDto?>.Success(null));

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateCustomer()
    {
        var dto = CustomerDtoTests.CreateDto();
        var created = CustomerDtoTests.CreateValid();

        _createMock.Setup(s => s.ExecuteAsync(dto))
            .ReturnsAsync(UseCaseResponse<CustomerDto>.Success(created));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateCustomer()
    {
        var dto = CustomerDtoTests.UpdateDto();
        var updated = CustomerDtoTests.CreateValid();

        _updateMock.Setup(s => s.ExecuteAsync(It.IsAny<UpdateCustomerRequest>()))
            .ReturnsAsync(UseCaseResponse<CustomerDto>.Success(updated));

        var result = await _controller.Update(Guid.NewGuid(), dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteCustomer()
    {
        _deleteMock.Setup(s => s.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(UseCaseResponse<bool>.Success(true));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }
}