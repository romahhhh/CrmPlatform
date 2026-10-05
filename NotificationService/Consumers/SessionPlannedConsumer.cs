using MassTransit;
using NotificationService.Services;
using Shared.Events;

namespace NotificationService.Consumers
{
    public class SessionPlannedConsumer : IConsumer<SessionPlannedEvent>
    {
        private readonly IConsoleNotificationSender _sender;

        public SessionPlannedConsumer(IConsoleNotificationSender sender)
        {
            _sender = sender;
        }

        public Task Consume(ConsumeContext<SessionPlannedEvent> context)
        {
            var @event = context.Message;
            _sender.SendSessionReminder(@event.ScheduledAt, @event.DurationInMinutes);
            return Task.CompletedTask;
        }
    }
}
