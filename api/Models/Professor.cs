namespace api.Models;

public record Professor(
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    [MinLength(3), MaxLength(20)] string Name,
    [EmailAddress] string Email,
    [MinLength(8)] string Password,
    Department Department
);
