namespace NotificationService.Services
{
    public interface IConsoleNotificationSender
    {
        void SendNewUserNotification(string name, string email);
        void SendWelcomeEmail(string email);
        void SendSessionReminder(DateTime scheduledAt, int durationMinutes);
    }
}
