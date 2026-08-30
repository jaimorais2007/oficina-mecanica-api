using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using OficinaApi.Presentation.ExceptionFilters;
using Xunit;

namespace OficinaApi.Tests.UnitTests.ExceptionFilters;

public class ServiceOrderExceptionFilterTests
{
    private readonly Mock<ILogger<ServiceOrderExceptionFilter>> _loggerMock;
    private readonly ServiceOrderExceptionFilter _filter;

    public ServiceOrderExceptionFilterTests()
    {
        _loggerMock = new Mock<ILogger<ServiceOrderExceptionFilter>>();
        _filter = new ServiceOrderExceptionFilter(_loggerMock.Object);
    }

    [Fact]
    public void OnException_ShouldLogStandardErrorWithEndpointVariablesAndSetBadRequestResult()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/ServiceOrders/c7b6d19b-2a91-4d45-9831-f123456789ab/start-analysis";

        var actionDescriptor = new ActionDescriptor
        {
            DisplayName = "ServiceOrdersController.MoveToAnalysis",
            RouteValues = new Dictionary<string, string?>
            {
                ["controller"] = "ServiceOrders",
                ["action"] = "MoveToAnalysis"
            }
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var expectedException = new InvalidOperationException("A ordem de serviço deve estar no status 'Recebida' para iniciar a análise técnica.");

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = expectedException
        };

        // Act
        _filter.OnException(exceptionContext);

        // Assert
        exceptionContext.ExceptionHandled.Should().BeTrue();
        exceptionContext.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = (BadRequestObjectResult)exceptionContext.Result!;
        badRequestResult.Value.Should().NotBeNull();

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("[ServiceOrder] Erro ao processar requisição no endpoint") &&
                    v.ToString()!.Contains("POST") &&
                    v.ToString()!.Contains("/api/ServiceOrders/c7b6d19b-2a91-4d45-9831-f123456789ab/start-analysis")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Attribute_ShouldCreateTypeFilterWithServiceOrderExceptionFilterType()
    {
        // Arrange & Act
        var attribute = new ServiceOrderExceptionFilterAttribute();

        // Assert
        attribute.ImplementationType.Should().Be(typeof(ServiceOrderExceptionFilter));
    }
}
