using Shared.Events;

namespace UserService.Messaging
{
    public interface IUserEventPublisher
    {
        Task PublishUserRegisteredAsync(UserRegisteredEvent @event, CancellationToken ct = default);
    }
}
