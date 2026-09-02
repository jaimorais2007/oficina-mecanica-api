using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OficinaApi.Presentation.Middlewares;

public class UserSessionEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserSessionEnrichmentMiddleware> _logger;

    public UserSessionEnrichmentMiddleware(RequestDelegate next, ILogger<UserSessionEnrichmentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = GetUserId(context);
        var sessionId = GetSessionId(context);

        var activity = Activity.Current;
        if (activity != null)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                activity.SetTag("user.id", userId);
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                activity.SetTag("session.id", sessionId);
            }
        }

        var scopeProperties = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(userId))
        {
            scopeProperties["user.id"] = userId;
            scopeProperties["UserId"] = userId;
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            scopeProperties["session.id"] = sessionId;
            scopeProperties["SessionId"] = sessionId;
        }

        if (scopeProperties.Count > 0)
        {
            using (_logger.BeginScope(scopeProperties))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private static string? GetUserId(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static string? GetSessionId(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return null;
        }

        var authHeaderValue = authHeader.ToString();
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authHeaderValue["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
