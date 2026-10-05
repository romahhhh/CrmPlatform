using MassTransit;
using Shared.Events;

namespace CRMService.Messaging
{
    public class CrmEventPublisher : ICrmEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CrmEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishClientCreatedAsync(ClientCreatedEvent @event, CancellationToken ct = default)
            => _publishEndpoint.Publish(@event, ct);

        public Task PublishSessionPlannedAsync(SessionPlannedEvent @event, CancellationToken ct = default)
            => _publishEndpoint.Publish(@event, ct);
    }
}
