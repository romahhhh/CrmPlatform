using CRMService.Models;
using System.ComponentModel.DataAnnotations;

namespace CRMService.DTOs
{
    public record SessionUpdateRequest(
        [Required] DateTime ScheduledAt,
        [Required, Range(1, 1440)] int DurationInMinutes,
        [Required] SessionStatus Status,
        [MaxLength(2000)] string? Notes
    );
}
