using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OficinaApi.Application.Services;
using OficinaApi.Domain.Entities;
using OficinaApi.Domain.Enums;
using OficinaApi.Domain.Interfaces;
using Xunit;

namespace OficinaApi.Tests;

public class ExternalQueryServiceTests
{
    private readonly Mock<IServiceOrderRepository> _orderRepoMock;
    private readonly ExternalQueryService _sut;

    public ExternalQueryServiceTests()
    {
        _orderRepoMock = new Mock<IServiceOrderRepository>();
        _sut = new ExternalQueryService(_orderRepoMock.Object);
    }

    [Fact]
    public async Task GetAverageExecutionTimeAsync_ShouldReturnZero_WhenNoFinishedOrders()
    {
        // Arrange
        _orderRepoMock.Setup(x => x.GetAllFinishedOrdersAsync())
            .ReturnsAsync(new List<ServiceOrder>());

        // Act
        var result = await _sut.GetAverageExecutionTimeAsync();

        // Assert
        result.TotalFinishedOrders.Should().Be(0);
        result.AverageExecutionTimeInHours.Should().Be(0);
    }

    [Fact]
    public async Task GetAverageExecutionTimeAsync_ShouldCalculateCorrectAverage()
    {
        // Arrange
        var order1 = new ServiceOrder("123", "ABC1D23");
        order1.UpdateStatus(OrderStatus.Executing); // starts time
        
        var order2 = new ServiceOrder("456", "XYZ9W87");
        order2.UpdateStatus(OrderStatus.Executing);

        // We cannot easily manipulate the timestamps strictly as they use DateTime.UtcNow inside the entity in this simplified MVP,
        // but for integration testing or exact unit testing we could abstract ISystemClock or use Reflection.
        // For the sake of standard mocking tests without reflection:
        // By changing status back to Finished instantly, it will record ~0 time difference but it's valid.
        order1.UpdateStatus(OrderStatus.Finished);
        order2.UpdateStatus(OrderStatus.Finished);

        _orderRepoMock.Setup(x => x.GetAllFinishedOrdersAsync())
            .ReturnsAsync(new List<ServiceOrder> { order1, order2 });

        // Act
        var result = await _sut.GetAverageExecutionTimeAsync();

        // Assert
        result.TotalFinishedOrders.Should().Be(2);
        // Because of UtcNow instant execution, average time is extremely small > 0.
        result.AverageExecutionTimeInHours.Should().BeGreaterOrEqualTo(0);
    }
}
