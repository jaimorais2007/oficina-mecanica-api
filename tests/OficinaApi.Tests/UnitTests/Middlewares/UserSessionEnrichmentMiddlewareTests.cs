using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OficinaApi.Presentation.Middlewares;
using Xunit;

namespace Unit.Tests;

public class UserSessionEnrichmentMiddlewareTests
{
    private readonly Mock<ILogger<UserSessionEnrichmentMiddleware>> _loggerMock;

    public UserSessionEnrichmentMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<UserSessionEnrichmentMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsAuthenticatedWithBearerToken_ShouldEnrichActivityAndCallNext()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var rawToken = "my-sample-jwt-token";
        var expectedSessionId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId) };
        var identity = new ClaimsIdentity(claims, "Bearer");
        httpContext.User = new ClaimsPrincipal(identity);
        httpContext.Request.Headers["Authorization"] = $"Bearer {rawToken}";

        using var activitySource = new ActivitySource("TestActivitySource");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = activitySource.StartActivity("TestActivity");

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserSessionEnrichmentMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        nextCalled.Should().BeTrue();
        activity.Should().NotBeNull();
        activity!.GetTagItem("user.id").Should().Be(userId);
        activity.GetTagItem("session.id").Should().Be(expectedSessionId);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserIsAnonymousAndNoBearerToken_ShouldNotSetTagsAndCallNext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        using var activitySource = new ActivitySource("TestActivitySource");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = activitySource.StartActivity("TestActivity");

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserSessionEnrichmentMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        nextCalled.Should().BeTrue();
        activity.Should().NotBeNull();
        activity!.GetTagItem("user.id").Should().BeNull();
        activity.GetTagItem("session.id").Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenUserHasNameIdentifierClaim_ShouldFallbackToNameIdentifier()
    {
        // Arrange
        var userId = "user-123";
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "Bearer");
        httpContext.User = new ClaimsPrincipal(identity);

        using var activitySource = new ActivitySource("TestActivitySource");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = activitySource.StartActivity("TestActivity");

        var middleware = new UserSessionEnrichmentMiddleware(_ => Task.CompletedTask, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        activity!.GetTagItem("user.id").Should().Be(userId);
    }
}
