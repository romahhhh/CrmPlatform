namespace NotificationService.Services
{
    public class ConsoleNotificationSender : IConsoleNotificationSender
    {
        private readonly ILogger<ConsoleNotificationSender> _logger;
        public ConsoleNotificationSender(ILogger<ConsoleNotificationSender> logger)
        {
            _logger = logger;
        }

        public void SendNewUserNotification(string name, string email)
        {
            _logger.LogInformation("[Notification] New user registered: {Name} ({Email})", name, email);

            Console.WriteLine($"[Notification] New user registered: {name} ({email})");
        }

        public void SendWelcomeEmail(string email)
        {
            _logger.LogInformation("[Notification] Welcome email sent to {Email}", email);

            Console.WriteLine($"[Notification] Welcome email sent to {email}");
        }

        public void SendSessionReminder(DateTime scheduledAt, int durationMinutes)
        {
            _logger.LogInformation(
           "[Notification] Reminder for session {ScheduledAt:dd.MM.yyyy HH:mm} ({Duration} min)", scheduledAt, durationMinutes);

            Console.WriteLine($"[Notification] Reminder for session {scheduledAt:dd.MM.yyyy HH:mm} ({durationMinutes} min)");
        }
    }
}
