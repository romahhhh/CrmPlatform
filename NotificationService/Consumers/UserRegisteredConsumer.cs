using MassTransit;
using NotificationService.Services;
using Shared.Events;

namespace NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IConsoleNotificationSender _sender;

        public UserRegisteredConsumer(IConsoleNotificationSender sender)
        {
            _sender = sender;
        }

        public Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var @event = context.Message;
            _sender.SendNewUserNotification(@event.Name, @event.Email);
            return Task.CompletedTask;
        }
    }
}
