using MassTransit;
using Shared.Events;

namespace UserService.Messaging
{
    public class UserEventPublisher : IUserEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public UserEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;                        
        }

        public Task PublishUserRegisteredAsync(UserRegisteredEvent @event, CancellationToken ct = default) => _publishEndpoint.Publish(@event, ct);
    }
}
