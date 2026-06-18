using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Events;

namespace OficinaApi.Infrastructure.Data;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        using var discoveryScope = _serviceProvider.CreateScope();
        var discoveredHandlers = discoveryScope.ServiceProvider.GetServices(handlerType).Cast<object>().ToList();
        var handlersCount = discoveredHandlers.Count;

        if (handlersCount == 0)
        {
            _logger.LogDebug("No domain event handlers found for event {EventName}", domainEvent.EventName);
            return;
        }

        var tasks = Enumerable.Range(0, handlersCount)
            .Select(index => InvokeHandlerInNewScopeAsync(handlerType, index, domainEvent, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task InvokeHandlerInNewScopeAsync(Type handlerInterfaceType, int index, DomainEvent domainEvent, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        try
        {
            var services = scope.ServiceProvider.GetServices(handlerInterfaceType).Cast<object>().ToList();

            if (index < 0 || index >= services.Count)
            {
                _logger.LogWarning("Handler index {Index} out of range for event {EventName}", index, domainEvent.EventName);
                return;
            }

            var handler = services[index];
            await ((dynamic)handler).HandleAsync((dynamic)domainEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in domain event handler while handling event {EventName}", domainEvent.EventName);
        }
    }
}
