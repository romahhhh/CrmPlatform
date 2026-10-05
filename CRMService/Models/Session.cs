namespace CRMService.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationInMinutes { get; set; }
        public SessionStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Client Client { get; set; } = null!;
    }
}
