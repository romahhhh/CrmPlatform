using MassTransit;
using NotificationService.Services;
using Shared.Events;

namespace NotificationService.Consumers
{
    public class ClientCreatedConsumer : IConsumer<ClientCreatedEvent>
    {
        private readonly IConsoleNotificationSender _sender;

        public ClientCreatedConsumer(IConsoleNotificationSender sender)
        {
            _sender = sender;
        }

        public Task Consume(ConsumeContext<ClientCreatedEvent> context)
        {
            var @event = context.Message;
            _sender.SendWelcomeEmail(@event.Email);
            return Task.CompletedTask;
        }
    }
}
