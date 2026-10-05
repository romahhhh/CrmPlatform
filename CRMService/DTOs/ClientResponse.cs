namespace CRMService.DTOs
{
    public record ClientResponse(
        Guid Id,
        string Name,
        string Email,
        string Phone,
        DateTime CreatedAt
    );
}
