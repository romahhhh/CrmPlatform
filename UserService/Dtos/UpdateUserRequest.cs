using System.ComponentModel.DataAnnotations;

namespace UserService.Dtos
{
    public record UpdateUserRequest(
      [Required, EmailAddress, MaxLength(256)] string Email,
      [Required, MinLength(6), MaxLength(100)] string Password,
      [Required, MaxLength(256)] string Name
    );
}
