using Moq;
using OficinaApi.Application.EventHandlers;
using OficinaApi.Application.Interfaces;
using OficinaApi.Domain.Events;
using Xunit;

namespace Unit.Tests;

public class ServiceOrderApprovedEventHandlerTests
{
    private readonly Mock<IUseCase<ServiceOrderApprovedEvent, bool>> _useCaseMock;
    private readonly ServiceOrderApprovedEventHandler _sut;

    public ServiceOrderApprovedEventHandlerTests()
    {
        _useCaseMock = new Mock<IUseCase<ServiceOrderApprovedEvent, bool>>();
        _sut = new ServiceOrderApprovedEventHandler(_useCaseMock.Object);
    }

    [Fact]
    public async Task HandleAsync_DeveChamarExecuteAsyncDoUseCase()
    {
        // Arrange
        var evento = new ServiceOrderApprovedEvent(Guid.NewGuid());

        // Act
        await _sut.HandleAsync(evento, CancellationToken.None);

        // Assert
        _useCaseMock.Verify(u => u.ExecuteAsync(evento), Times.Once);
    }
}
