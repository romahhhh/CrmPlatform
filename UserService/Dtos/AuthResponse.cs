namespace UserService.Dtos
{
  public record AuthResponse(
      Guid UserId,
      string Email,
      string Name,
      string Token
  );
}
