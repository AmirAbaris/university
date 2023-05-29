namespace api.Models;

public record Student(
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)] string? Id,
    [MinLength(3), MaxLength(20)] string Name,
    [Range(18, 99)] int Age,
    [EmailAddress] string Email,
    [MinLength(8)] string Password
);
