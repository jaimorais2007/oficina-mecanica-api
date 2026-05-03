using System;

namespace OficinaApi.Domain.Events;

public abstract class DomainEvent
{
	public DateTime OccurredOn { get; } = DateTime.UtcNow;
	public string EventName { get; }

	protected DomainEvent()
	{
		EventName = GetType().Name;
	}
}
