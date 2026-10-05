using CRMService.Models;

namespace CRMService.DTOs
{
    public record SessionResponse(
        Guid Id,
        Guid ClientId,
        DateTime ScheduledAt,
        int DurationInMinutes,
        SessionStatus Status,
        string? Notes
    );
}
