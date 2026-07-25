namespace CriaCerto.Modules.Tenancy.Application.Contracts;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber
);
