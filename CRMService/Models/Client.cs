namespace CRMService.Models
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
