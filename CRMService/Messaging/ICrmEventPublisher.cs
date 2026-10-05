using Shared.Events;

namespace CRMService.Messaging
{
    public interface ICrmEventPublisher
    {
        Task PublishClientCreatedAsync(ClientCreatedEvent @event, CancellationToken ct = default);
        Task PublishSessionPlannedAsync(SessionPlannedEvent @event, CancellationToken ct = default);
    }
}
