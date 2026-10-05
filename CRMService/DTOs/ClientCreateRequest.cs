using System.ComponentModel.DataAnnotations;

namespace CRMService.DTOs
{
    public record ClientCreateRequest(
        [Required, MaxLength(256)] string Name,
        [Required, EmailAddress, MaxLength(256)] string Email,
        [Required, MaxLength(50)] string Phone
    );
}
