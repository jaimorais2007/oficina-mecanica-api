using Microsoft.Extensions.Logging;

namespace OficinaApi.WebApi.Configuration;

public static class LoggingConfiguration
{
    public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
        });

        return services;
    }
}
