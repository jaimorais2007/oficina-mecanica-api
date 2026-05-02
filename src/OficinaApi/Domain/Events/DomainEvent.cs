using System;

namespace OficinaApi.Domain.Events;

public abstract class DomainEvent
{
	public DateTime OccurredOn { get; }
	public string EventName { get; } = string.Empty;
}
